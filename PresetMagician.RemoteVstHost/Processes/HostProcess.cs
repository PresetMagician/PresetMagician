using System;
using System.Diagnostics;
using System.Timers;
using Catel;
using Catel.Data;
using Catel.Logging;
using Catel.Threading;
using Drachenkatze.PresetMagician.Utils;
using MethodTimer;
using SharedModels;
using Timer = System.Timers.Timer;

namespace PresetMagician.RemoteVstHost.Processes
{
    public class HostProcess : ObservableObject
    {
        public enum ProcessState
        {
            STARTING,
            RUNNING,
            EXITED
        }

        public int Pid { get; private set; }
        public DateTime StartDateTime { get; private set; }

        public string StartupTimeFormatted
        {
            get { return (int) StartupTime.TotalMilliseconds + "ms"; }
        }

        public DateTime StopDateTime { get; private set; }
        public long MemoryUsage { get; private set; }


        public ProcessState CurrentProcessState
        {
            get => _currentProcessState;
            private set
            {
                _currentProcessState = value;
                ProcessStateUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public string Logs
        {
            get { return string.Join(Environment.NewLine, Logger.LogList); }
        }

        public MiniMemoryLogger Logger { get; }


        public bool IsBusy
        {
            get => _isBusy;
            protected set { _isBusy = value; }
        }


        public TimeSpan StartupTime { get; private set; }
        public TimeSpan Uptime { get; private set; }
        public string StopReason { get; private set; }

        public bool StartupSuccessful { get; private set; }

        private readonly int _maxStartupTime;
        private const int UpdateInterval = 1000;
        private const string ProcessName = "PresetMagician.RemoteVstHost.exe";

        private readonly Timer _startupTimer;
        private readonly Timer _updateTimer;

        private readonly string _processImageName;
        private Process _process;
        protected bool inShutdown;
        private ProcessState _currentProcessState;
        private bool _isBusy;
        private bool _debug;

        public event EventHandler ProcessStateUpdated;


        [Time]
        public HostProcess(int maxStartupTimeSeconds = 20, bool debug=false)
        {
            _debug = debug;
            Logger = new MiniMemoryLogger(debug);
            _maxStartupTime = maxStartupTimeSeconds * 1000;
            _processImageName = ProcessName;

            var processStartInfo = new ProcessStartInfo(_processImageName)
            {
                CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            CurrentProcessState = ProcessState.STARTING;

            _process = new Process();

            _process.StartInfo = processStartInfo;
            _process.OutputDataReceived += ProcessOnOutputDataReceived;
            _process.Exited += ProcessOnExited;
            _process.EnableRaisingEvents = true;

            _startupTimer = new Timer {Interval = _maxStartupTime, Enabled = false, AutoReset = false};
            _startupTimer.Elapsed += StartupTimerOnElapsed;

            _updateTimer = new Timer {Interval = UpdateInterval, Enabled = false, AutoReset = false};
            _updateTimer.Elapsed += UpdateTimerOnElapsed;
        }

        public virtual void Start()
        {
            try
            {
                _process.Start();
                _process.BeginOutputReadLine();
                Pid = _process.Id;
                StartDateTime = DateTime.Now;

                IsBusy = true;
                _startupTimer.Start();
                _updateTimer.Start();
            }

            catch (Exception e)
            {
                Log($"Unable to start {_processImageName}), got exception {e}");
                CurrentProcessState = ProcessState.EXITED;
                StopDateTime = DateTime.Now;
            }
        }


        private void UpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (_process.HasExited)
            {
                return;
            }

            _process.Refresh();

            try
            {
                MemoryUsage = _process.PrivateMemorySize64;
            }
            catch (Exception)
            {
                // The process has probably gone away during retrieval of PrivateMemorySize64
            }


            Uptime = DateTime.Now - StartDateTime;

            if (CurrentProcessState == ProcessState.STARTING)
            {
                StartupTime = DateTime.Now - StartDateTime;
            }

            _updateTimer.Start();
        }

        private void StartupTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (CurrentProcessState == ProcessState.STARTING)
            {
                ForceStop($"Host took longer than {_maxStartupTime / 1000} seconds to start up, assuming it hangs.");
            }
        }

        [Time]
        protected void Log(string message)
        {
            Logger.Debug(message);
        }

        [Time]
        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            if (e.Data.Trim() == $"{_processImageName}:{Pid} ready.")
            {
                CurrentProcessState = ProcessState.RUNNING;
                StartupTime = DateTime.Now - StartDateTime;
                StartupSuccessful = true;
                _startupTimer.Stop();
                TaskHelper.Run(() => { OnAfterStart(); });
            }
            else
            {
                Logger.Debug($"Console {e.Data}");
                OnOutputDataReceived(e.Data);
            }

            if (_debug)
            {
                Debug.WriteLine(e.Data);
            }
        }

        protected virtual void OnOutputDataReceived(string data)
        {
            
        }

        protected virtual void OnAfterStart()
        {
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            if (CurrentProcessState == ProcessState.EXITED)
            {
                // Process is already existed, do nothing
                return;
            }

            Log($"{_processImageName} shut itself down (timeout?), cleaning up");

            StopDateTime = DateTime.Now;
            CurrentProcessState = ProcessState.EXITED;

            CleanupBeforeForceStop();
            StopReason = "Process stopped itself";
        }

        protected virtual void OnBeforeForceStop()
        {
        }

        private void CleanupBeforeForceStop()
        {
            _startupTimer.Stop();
            _updateTimer.Stop();
            _process.Exited -= ProcessOnExited;

            try
            {
                OnBeforeForceStop();
            }
            catch (Exception e)
            {
                Log($"Tried to cleanup before stop, but got {e.GetType().FullName} {e.Message}");
            }
        }

        public void ForceStop(string reason)
        {
            inShutdown = true;

            if (CurrentProcessState != ProcessState.EXITED)
            {
                Log($"Instructing {Pid} to stop because of {reason}");
                CurrentProcessState = ProcessState.EXITED;


                CleanupBeforeForceStop();
            }

            if (!_process.HasExited)
            {
                _process.Kill();
            }

            StopReason = reason;
            StopDateTime = DateTime.Now;
        }
    }
}