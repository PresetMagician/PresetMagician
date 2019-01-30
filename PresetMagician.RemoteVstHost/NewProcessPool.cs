using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Catel;
using Catel.Collections;
using MethodTimer;
using PresetMagician.ProcessIsolation.Processes;
using SharedModels;

namespace PresetMagician.ProcessIsolation
{
    public class NewProcessPool
    {
        protected virtual string BaseAddress { get; } = "net.pipe://localhost/presetmagician/vstService/";

        private int _maxProcesses = 8;
        private int _maxStartTimeout = 10;
        private int _failedStartupProcesses;
        private int _totalStartedProcesses;
        public event EventHandler ProcessWatcherUpdated;
        public event EventHandler<PoolFailedEventArgs> PoolFailed;

        public FastObservableCollection<HostProcess> RunningProcesses { get; } =
            new FastObservableCollection<HostProcess>();

        public FastObservableCollection<HostProcess> OldProcesses { get; } =
            new FastObservableCollection<HostProcess>();

        private readonly Timer _processWatcher;
        private bool _poolRunning;

        private static readonly object _updateLock = new object();

        public NewProcessPool()
        {
            _processWatcher = new Timer(500) {AutoReset = false};
            _processWatcher.Elapsed += ProcessWatcherOnElapsed;
        }

        public void StartPool()
        {
            lock (_updateLock)
            {
                _poolRunning = true;
                _processWatcher.Start();
            }
        }

        public void SetMaxProcesses(int maxProcesses)
        {
            _maxProcesses = maxProcesses;
        }

        public void SetStartTimeout(int maxStartTimeoutSeconds)
        {
            _maxStartTimeout = maxStartTimeoutSeconds;
        }

        public void StopPool()
        {
            lock (_updateLock)
            {
                using (RunningProcesses.SuspendChangeNotifications())
                {
                    _failedStartupProcesses = 0;
                    _poolRunning = false;
                    _processWatcher.Stop();
                    foreach (var process in RunningProcesses.ToArray())
                    {
                        process.ForceStop("Pool shutdown");
                    }
                }
            }
        }

        private void ProcessWatcherOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (!_poolRunning)
            {
                return;
            }

            UpdateProcesses();
            ProcessWatcherUpdated.SafeInvoke(this);
            _processWatcher.Start();
        }

        private void UpdateProcesses()
        {
            lock (_updateLock)
            {
                using (RunningProcesses.SuspendChangeNotifications())
                using (OldProcesses.SuspendChangeNotifications())
                {
                    var pluginsToRemove = new List<HostProcess>();
                    foreach (var process in RunningProcesses)
                    {
                        if (process.CurrentProcessState == HostProcess.ProcessState.EXITED)
                        {
                            UnwatchProcess(process);
                            pluginsToRemove.Add(process);
                            OldProcesses.Add(process);
                        }
                    }

                    foreach (var process in pluginsToRemove)
                    {
                        RunningProcesses.Remove(process);
                    }

                    if (_poolRunning)
                    {
                        if (RunningProcesses.Count < _maxProcesses)
                        {
                            for (var i = 0; i < _maxProcesses - RunningProcesses.Count; i++)
                            {
                                var process = new HostProcess(_maxStartTimeout);
                                process.BusyUpdated += ProcessOnBusyUpdated;
                                process.ProcessStateUpdated += ProcessOnProcessStateUpdated;
                                process.OperationUpdated += ProcessOnOperationUpdated;
                                RunningProcesses.Add(process);
                                _totalStartedProcesses++;
                            }
                        }
                    }
                }
            }
        }


        private void UnwatchProcess(HostProcess process)
        {
            process.BusyUpdated -= ProcessOnBusyUpdated;
            process.ProcessStateUpdated -= ProcessOnProcessStateUpdated;
            process.ProcessStateUpdated -= ProcessOnOperationUpdated;
        }

        private void ProcessOnProcessStateUpdated(object sender, EventArgs e)
        {
            lock (_updateLock)
            {
                var process = sender as HostProcess;
                if (process.CurrentProcessState == HostProcess.ProcessState.EXITED && !process.StartupSuccessful && _poolRunning)
                {
                    _failedStartupProcesses++;
                    if (_failedStartupProcesses > 5 && (double) _failedStartupProcesses / _totalStartedProcesses > 0.1)
                    {
                        StopPool();
                        var eventArgs = new PoolFailedEventArgs
                        {
                            ShutdownReason =
                                "The VST worker pool has been stopped because: " + Environment.NewLine +
                                Environment.NewLine +
                                "More than 10% of all VST process workers failed to startup. Check the logs for each failed " +
                                " worker (Tab 'VST Workers') and increase the startup time in the settings if necessary. " +
                                Environment.NewLine + Environment.NewLine +
                                "Re-start your pool after investigation and configuration."

                        };
                        PoolFailed.SafeInvoke(this, eventArgs);
                    }
                }
            }

            UpdateProcesses();
        }

        private void ProcessOnBusyUpdated(object sender, EventArgs e)
        {
            ProcessWatcherUpdated.SafeInvoke(this);
        }

        private void ProcessOnOperationUpdated(object sender, EventArgs e)
        {
            ProcessWatcherUpdated.SafeInvoke(this);
        }
    }

    public class PoolFailedEventArgs : EventArgs
    {
        public string ShutdownReason { get; set; }
    }
}