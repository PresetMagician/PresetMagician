using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
        
        public static ObservableCollection<IsolatedProcess> Processes = new ObservableCollection<IsolatedProcess>();
        private Timer _processWatcher;

        public ProcessPool()
        {
            _processWatcher = new Timer(1000);
            _processWatcher.AutoReset = false;
            _processWatcher.Elapsed += ProcessWatcherOnElapsed;
            _processWatcher.Start();

            CreateProcesses();
        }

        private void ProcessWatcherOnElapsed(object sender, ElapsedEventArgs e)
        {
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
                    if (process.CurrentProcessState == IsolatedProcess.ProcessState.EXITED || process.CurrentProcessState == IsolatedProcess.ProcessState.KILLED)
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

        public static void KillRemotevstService()
        {
            _currentProcess.Kill();
        }
    }

    public class IsolatedProcess
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
                CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true
            };

            CurrentProcessState = ProcessState.STARTING;
            _process = Process.Start(processStartInfo);
            _process.BeginOutputReadLine();
            _process.OutputDataReceived += ProcessOnOutputDataReceived;

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

            ((IClientChannel)_vstService).Faulted += OnFaulted;
            ((IClientChannel)_vstService).Closed += OnFaulted;
            
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
            CurrentProcessState = ProcessState.KILLED;
            ((IClientChannel)_vstService).Abort();
            Kill();
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            LogTo.Debug($"Plugin host worker {Pid}: "+e.Data);
        }

        public IRemoteVstService GetVstService()
        {
            return _vstService;
        }

        private void StartupTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _vstService.Ping();
            CurrentProcessState = ProcessState.RUNNING;
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            CurrentProcessState = ProcessState.EXITED;
        }

        public void Kill()
        {
            if (!_process.HasExited)
            {
                _process.Kill();
                CurrentProcessState = ProcessState.KILLED;
            }
        }

      
    }
}