using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Timers;
using Anotar.Catel;
using Catel;
using SharedModels;

namespace PresetMagician.ProcessIsolation
{
    public class ProcessPool
    {
        public static string BaseAddress = "net.pipe://localhost/presetmagician/vstService/";

        private static IsolatedProcess _currentProcess;
        private const int MAX_PROCESSES = 4;
        public event EventHandler ProcessWatcherUpdated;

        public static readonly ObservableCollection<IsolatedProcess> Processes =
            new ObservableCollection<IsolatedProcess>();

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
            lock (Processes)
            {
                _poolRunning = false;
                _processWatcher.Stop();
                foreach (var process in Processes.ToArray())
                {
                    process.Kill();
                }

                Processes.Clear();
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
            lock (Processes)
            {
                foreach (var process in Processes.ToArray())
                {
                    if (process.CurrentProcessState == IsolatedProcess.ProcessState.EXITED ||
                        process.CurrentProcessState == IsolatedProcess.ProcessState.KILLED)
                    {
                        Processes.Remove(process);
                    }
                }

                if (Processes.Count < MAX_PROCESSES)
                {
                    for (var i = 0; i < MAX_PROCESSES - Processes.Count; i++)
                    {
                        Processes.Add(new IsolatedProcess());
                    }
                }

                if (_currentProcess?.CurrentProcessState != IsolatedProcess.ProcessState.RUNNING)
                {
                    _currentProcess = null;
                }
            }
        }

        private static async Task<bool> EnsureCurrentProcess()
        {
            if (_currentProcess != null && _currentProcess.CurrentProcessState == IsolatedProcess.ProcessState.RUNNING)
            {
                return true;
            }

            lock (Processes)
            {
                foreach (var process in Processes)
                {
                    if (process.CurrentProcessState != IsolatedProcess.ProcessState.RUNNING)
                    {
                        continue;
                    }

                    _currentProcess = process;
                    return true;
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            return false;
        }

        public static async Task<IRemoteVstService> GetRemoteVstService()
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

        public static void KillRemotevstService()
        {
            _currentProcess.Kill();
        }
    }

    public interface IIsolatedProcess
    {
        void Kill();
        IRemoteVstService GetVstService();
    }

    public class IsolatedProcess : IIsolatedProcess
    {
        public enum ProcessState
        {
            STARTING,
            RUNNING,
            KILLED,
            EXITED
        }

        public int Pid { get; private set; }
        private Timer _startupTimer;
        private Process _process;
        private readonly IRemoteVstService _vstService;
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

            var address = ProcessPool.BaseAddress + _process.Id;
            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
            {
                OpenTimeout = new TimeSpan(0, 0, 0, 0, 300),
                MaxReceivedMessageSize = 40000000,
                SendTimeout = new TimeSpan(0, 0, 0, 60)
            };

            Pid = _process.Id;

            EndpointAddress ep = new EndpointAddress(address);

            _vstService = ChannelFactory<IRemoteVstService>.CreateChannel(binding, ep);

            ((IClientChannel) _vstService).Faulted += OnFaulted;
            ((IClientChannel) _vstService).Closed += OnFaulted;
            ((IClientChannel) _vstService).Closing += OnFaulted;

            _process.EnableRaisingEvents = true;
            _process.Exited += ProcessOnExited;


            _startupTimer = new Timer();
            _startupTimer.Elapsed += StartupTimerOnElapsed;
            _startupTimer.Interval = 500;
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
            _startupTimer.Stop();
            CurrentProcessState = ProcessState.KILLED;
            Kill();
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                LogTo.Debug($"Plugin host worker {Pid}: " + e.Data);
            }
        }

        public IRemoteVstService GetVstService()
        {
            return _vstService;
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
            catch (Exception ex) when (ex is CommunicationException cex)
            {
                Kill();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            _startupTimer.Stop();
            ((IClientChannel) _vstService).Abort();
            CurrentProcessState = ProcessState.EXITED;
        }

        public void Kill()
        {
            ((IClientChannel) _vstService).Abort();
            _startupTimer.Stop();

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