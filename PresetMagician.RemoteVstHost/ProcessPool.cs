using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Timers;
using Anotar.Catel;
using Catel;
using Catel.Collections;
using Catel.Data;
using PresetMagician.ProcessIsolation.Services;
using SharedModels;

namespace PresetMagician.ProcessIsolation
{
    public class ProcessPool
    {
        public static string BaseAddress = "net.pipe://localhost/presetmagician/vstService/";

        private static IsolatedProcess _currentProcess;
        private const int MAX_PROCESSES = 4;
        public event EventHandler ProcessWatcherUpdated;

        public static readonly FastObservableCollection<IsolatedProcess> RunningProcesses =
            new FastObservableCollection<IsolatedProcess>();

        public static readonly FastObservableCollection<IsolatedProcess> OldProcesses =
            new FastObservableCollection<IsolatedProcess>();

        private readonly Timer _processWatcher;
        private bool _poolRunning;

        public ProcessPool()
        {
            _processWatcher = new Timer(1000) {AutoReset = false};
            _processWatcher.Elapsed += ProcessWatcherOnElapsed;
        }

        public void StartPool()
        {
            _poolRunning = true;
            _processWatcher.Start();
        }

        public void StopPool()
        {
            lock (RunningProcesses)
            {
                using (RunningProcesses.SuspendChangeNotifications())
                {
                    _poolRunning = false;
                    _processWatcher.Stop();
                    foreach (var process in RunningProcesses.ToArray())
                    {
                        process.Kill("Pool stop");
                    }

                    RunningProcesses.Clear();
                }
            }
        }

        private void ProcessWatcherOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (!_poolRunning)
            {
                return;
            }

            CreateProcesses();
            ProcessWatcherUpdated.SafeInvoke(this);
            _processWatcher.Start();
        }

        public static void CreateProcesses()
        {
            lock (RunningProcesses)
            {
                using (RunningProcesses.SuspendChangeNotifications())
                {
                        if (RunningProcesses.Count < MAX_PROCESSES)
                        {
                            for (var i = 0; i < MAX_PROCESSES - RunningProcesses.Count; i++)
                            {
                                var process = new IsolatedProcess();
                                process.PropertyChanged += ProcessOnPropertyChanged; 
                                RunningProcesses.Add(process);
                            }
                        }

                        if (_currentProcess?.CurrentProcessState != IsolatedProcess.ProcessState.RUNNING)
                        {
                            _currentProcess = null;
                        }
                    
                }
            }
        }

        private static void ProcessOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsolatedProcess.CurrentProcessState))
            {
                var process = sender as IsolatedProcess;
                if (process.CurrentProcessState == IsolatedProcess.ProcessState.EXITED ||
                    process.CurrentProcessState == IsolatedProcess.ProcessState.KILLED ||
                    process.CurrentProcessState == IsolatedProcess.ProcessState.KILLING)
                {
                    lock (RunningProcesses)
                    {
                        lock (OldProcesses)
                        {
                            using (RunningProcesses.SuspendChangeNotifications())
                            {
                                RunningProcesses.Remove(process);
                                process.PropertyChanged -= ProcessOnPropertyChanged;
                                OldProcesses.Add(process);
                            }
                        }
                    }
                }
            }
        }

        private static async Task<bool> EnsureCurrentProcess()
        {
            if (_currentProcess != null && _currentProcess.CurrentProcessState == IsolatedProcess.ProcessState.RUNNING)
            {
                return true;
            }

            lock (RunningProcesses)
            {
                foreach (var process in RunningProcesses)
                {
                    if (process.CurrentProcessState != IsolatedProcess.ProcessState.RUNNING)
                    {
                        continue;
                    }

                    _currentProcess = process;
                    return true;
                }
            }

            await Task.Delay(500);
            return false;
        }

        public static async Task<ProxiedRemoteVstService> GetRemoteVstService()
        {
            for (var i = 0; i < 10; i++)
            {
                if (await EnsureCurrentProcess())
                {
                    return _currentProcess.GetVstService();
                }
            }

            throw new Exception("Unable to find a running VST service");
        }

        public static async Task<IRemotePluginInstance> GetRemotePluginInstance(Plugin plugin)
        {
            for (var i = 0; i < 10; i++)
            {
                if (await EnsureCurrentProcess())
                {
                    return _currentProcess.GetRemotePluginInstance(plugin);
                }
            }

            throw new Exception("Unable to find a running VST service");
        }

        public static async Task<ProxiedRemoteVstService> GetRemoteFileService()
        {
            for (var i = 0; i < 10; i++)
            {
                if (await EnsureCurrentProcess())
                {
                    return _currentProcess.GetFileService();
                }
            }

            throw new Exception("Unable to find a running VST service");
        }

    }

    public interface IIsolatedProcess
    {
        void Kill(string reason);
        ProxiedRemoteVstService GetVstService();

        int Pid { get; }
    }

    public class IsolatedProcess : ObservableObject, IIsolatedProcess 
    {
        public enum ProcessState
        {
            STARTING,
            RUNNING,
            KILLING,
            KILLED,
            EXITED
        }

        public int Pid { get; private set; }
        private Timer _startupTimer;
        private Process _process;
        private readonly IRemoteVstService _vstService;
        private ProxiedRemoteVstService _proxiedVstService;
        private string _address;
        public ProcessState CurrentProcessState { get; private set; }

        public IsolatedProcess()
        {
            var processStartInfo = new ProcessStartInfo("PresetMagician.RemoteVstHost.exe")
            {
                CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            CurrentProcessState = ProcessState.STARTING;
            _process = Process.Start(processStartInfo);
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            _process.OutputDataReceived += ProcessOnOutputDataReceived;
            _process.ErrorDataReceived += ProcessOnOutputDataReceived;

            
            _address = ProcessPool.BaseAddress + _process.Id;
            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
            {
                OpenTimeout = new TimeSpan(0, 0, 0, 0, 300),
                MaxReceivedMessageSize = 40000000,
                SendTimeout = new TimeSpan(0, 0, 0, 60)
            };

            Pid = _process.Id;

            EndpointAddress ep = new EndpointAddress(_address);

            _vstService = ChannelFactory<IRemoteVstService>.CreateChannel(binding, ep);

            ((IClientChannel) _vstService).Faulted += OnFaulted;
            ((IClientChannel) _vstService).Closed += OnFaulted;
            ((IClientChannel) _vstService).Closing += OnFaulted;

            _process.EnableRaisingEvents = true;
            _process.Exited += ProcessOnExited;


            _startupTimer = new Timer();
            _startupTimer.Elapsed += StartupTimerOnElapsed;
            _startupTimer.Interval = 10000;
            _startupTimer.Enabled = true;
            _startupTimer.AutoReset = false;
        }

        public async Task WaitForStartup()
        {
            for (var i = 0; i < 10; i++)
            {
                if (CurrentProcessState == ProcessState.RUNNING)
                {
                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            throw new Exception("Unable to find a running VST host process");
        }

        private void OnFaulted(object sender, EventArgs e)
        {
            if (CurrentProcessState == ProcessState.KILLING)
            {
                return;
            }
            _startupTimer.Stop();
            CurrentProcessState = ProcessState.KILLED;
            Kill("OnFaulted");
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                LogTo.Debug($"Plugin host worker {Pid}: " + e.Data);
                
                if (e.Data.Trim() == $"{_address} ready.")
                {
                    try
                    {
                        _vstService.Ping();
                        CurrentProcessState = ProcessState.RUNNING;
                    }
                    catch (Exception ex) 
                    {
                        Debug.WriteLine(ex);
                        Kill("Host says its started but it's not");
                    }
                }
                
            }
        }

        public ProxiedRemoteVstService GetVstService()
        {
            if (_proxiedVstService != null)
            {
                return _proxiedVstService;
            }

            _proxiedVstService = new ProxiedRemoteVstService(_vstService, this);

            return _proxiedVstService;
        }

        public ProxiedRemoteVstService GetFileService()
        {
            return GetVstService();
        }

        public IRemotePluginInstance GetRemotePluginInstance(Plugin plugin)
        {
            return new RemotePluginInstance(this, plugin);
        }

        private void StartupTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (CurrentProcessState == ProcessState.EXITED || CurrentProcessState == ProcessState.KILLED)
            {
                return;
            }

            try
            {
                _vstService.Ping();
                CurrentProcessState = ProcessState.RUNNING;
                _startupTimer.Interval = 10000;
                _startupTimer.Start();
            }
            catch (Exception ex) 
            {
                Debug.WriteLine(ex);
                Kill("ping error");
            }
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            _startupTimer.Stop();
            ((IClientChannel) _vstService).Abort();
            CurrentProcessState = ProcessState.EXITED;
        }

        public void Kill(string reason)
        {
            CurrentProcessState = ProcessState.KILLING;
            LogTo.Debug($"Instructing {Pid} to kill itself because of {reason}");
            _startupTimer.Stop();

            try
            {
                _vstService.KillSelf();
            }
            catch (Exception e)
            {
            }
            
            ((IClientChannel) _vstService).Abort();

            if (!_process.HasExited)
            {
                CurrentProcessState = ProcessState.KILLED;
                _process.Kill();
            }
            else
            {
                CurrentProcessState = ProcessState.EXITED;
            }
        }
    }
}