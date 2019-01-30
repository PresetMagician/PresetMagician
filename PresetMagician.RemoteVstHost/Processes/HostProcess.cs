using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using Anotar.Catel;
using Catel;
using Catel.Collections;
using Catel.Data;
using MethodTimer;

namespace PresetMagician.ProcessIsolation.Processes
{
    public class HostProcess : ObservableObject
    {
        public enum ProcessState
        {
            STARTING,
            RUNNING,
            EXITED
        }

        public int Pid { get; }
        public DateTime StartDateTime { get; }
        public DateTime StopDateTime { get; private set; }
        public long MemoryUsage { get; private set; }


        public ProcessState CurrentProcessState
        {
            get => _currentProcessState;
            private set
            {
                _currentProcessState = value;
                ProcessStateUpdated.SafeInvoke(this);
                
            }
        }

        public FastObservableCollection<string> LogMessages = new FastObservableCollection<string>();
        public FastObservableCollection<ProcessOperation> Operations = new FastObservableCollection<ProcessOperation>();

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                _isBusy = value;
                BusyUpdated.SafeInvoke(this);
            }
        }

        public ProcessOperation CurrentOperation { get; private set; }

        public TimeSpan StartupTime { get; private set; }
        public TimeSpan Uptime { get; private set; }

        public bool StartupSuccessful { get; private set; }

        private readonly int _maxStartupTime;
        private const int UpdateInterval = 100;
        private const string ProcessName = "PresetMagician.RemoteVstHost.exe";

        private readonly Timer _startupTimer;
        private readonly Timer _updateTimer;

        private readonly string _processImageName;
        private readonly Process _process;
        private readonly object _updateLock = new object();
        private ProcessState _currentProcessState;
        private bool _isBusy;

        public event EventHandler BusyUpdated;
        public event EventHandler ProcessStateUpdated;
        public event EventHandler OperationUpdated;

        public HostProcess(int maxStartupTimeSeconds = 10)
        {
            _maxStartupTime = maxStartupTimeSeconds * 1000;
            _processImageName = ProcessName;

            var processStartInfo = new ProcessStartInfo(_processImageName)
            {
                CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            CurrentProcessState = ProcessState.STARTING;

            try
            {
                StartOperation($"Starting up {_processImageName}");
                _process = Process.Start(processStartInfo);
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
                _process.OutputDataReceived += ProcessOnOutputDataReceived;
                _process.ErrorDataReceived += ProcessOnOutputDataReceived;
                _process.Exited += ProcessOnExited;
                _process.EnableRaisingEvents = true;

                Pid = _process.Id;
                StartDateTime = DateTime.Now;

                _startupTimer = new Timer {Interval = _maxStartupTime, Enabled = true, AutoReset = false};
                _startupTimer.Elapsed += StartupTimerOnElapsed;

                _updateTimer = new Timer {Interval = UpdateInterval, Enabled = true, AutoReset = true};
                _updateTimer.Elapsed += UpdateTimerOnElapsed;
            }
            catch (Exception e)
            {
                Log($"Unable to start {_processImageName}), got exception {e}");
                StopOperation($"Starting up {_processImageName}", "Error starting up");
                CurrentProcessState = ProcessState.EXITED;
                StopDateTime = DateTime.Now;
            }
        }

        protected void StartOperation(string operation)
        {
            lock (_updateLock)
            {
                if (CurrentProcessState == ProcessState.EXITED)
                {
                    LogTo.Error(
                        $"Error PID {Pid}: Attempted to start operation {operation} but the process has already stopped!");
                }

                if (IsBusy)
                {
                    LogTo.Error(
                        $"Error PID {Pid}: Attempted to start operation {operation} before another one finished!");
                }

                CurrentOperation = new ProcessOperation(operation, DateTime.Now);
                Operations.Add(CurrentOperation);
                IsBusy = true;
                OperationUpdated.SafeInvoke(this);
            }
        }

        protected void StopOperation(string operation, string result = "OK")
        {
            lock (_updateLock)
            {
                if (CurrentOperation == null)
                {
                    LogTo.Error(
                        $"Error PID {Pid}: Attempted to stop operation {operation} but there was no operation running!");
                    return;
                }

                if (CurrentOperation.Name != operation)
                {
                    LogTo.Error(
                        $"Error PID {Pid}: Attempted to stop operation {operation} but the current running operation is {CurrentOperation}");
                    CurrentOperation = null;
                    return;
                }

                IsBusy = false;
                CurrentOperation.SetStopTime(DateTime.Now);
                CurrentOperation.SetResult(result);
                CurrentOperation.SetCompleted();
                OperationUpdated.SafeInvoke(this);
            }
        }

        private void UpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (!_process.HasExited)
            {
                _process.Refresh();
                MemoryUsage = _process.PrivateMemorySize64 ;
            }
            
            Uptime = DateTime.Now - StartDateTime;

            if (CurrentProcessState == ProcessState.STARTING)
            {
                StartupTime = DateTime.Now - StartDateTime;
            }
        }

        private void StartupTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (CurrentProcessState == ProcessState.STARTING)
            {
                ForceStop($"Host took longer than {_maxStartupTime / 1000} seconds to start up, assuming it hangs.");
            }
        }

        protected void Log(string message)
        {
            LogMessages.Add($"{DateTime.Now.ToLongTimeString()} {message}");
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }
            
            Debug.WriteLine("received data length: "+e.Data.Length);
            Debug.WriteLine(e.Data);
            if (e.Data.Trim() == $"{_processImageName}:{Pid} ready.")
            {
                StopOperation($"Starting up {_processImageName}");
                
                var isRunning = false;

                lock (_updateLock)
                {
                    try
                    {
                        isRunning = OnVerifyRunning();
                        if (isRunning)
                        {
                            CurrentProcessState = ProcessState.RUNNING;
                            StartupSuccessful = true;
                            _startupTimer.Stop();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Tried to verify if the process is running, but got exception {ex.Message}");
                    }
                }

                if (!isRunning)
                {
                    ForceStop("Host says its started but it's not");
                }
            }
            else
            {
                Log(e.Data);
            }
        }

        protected virtual bool OnVerifyRunning()
        {
            return true;
        }

        private void ProcessOnExited(object sender, EventArgs e)
        {
            if (CurrentProcessState == ProcessState.EXITED)
            {
                // Process is already existed, do nothing
                return;
            }
            Log($"{_processImageName} shut itself down (timeout?), cleaning up");
            lock (_updateLock)
            {
                CurrentProcessState = ProcessState.EXITED;
                StopDateTime = DateTime.Now;
                CleanupBeforeForceStop();
            }
        }

        protected void OnBeforeForceStop()
        {
        }

        private void CleanupBeforeForceStop()
        {
            _startupTimer.Stop();
            _updateTimer.Stop();

            try
            {
                OnBeforeForceStop();
            }
            catch (Exception e)
            {
                Log($"Tried to cleanup before stop, but got exception {e.Message}");
            }
        }

        public void ForceStop(string reason)
        {
            lock (_updateLock)
            {
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

                StopDateTime = DateTime.Now;
            }
        }
    }
}