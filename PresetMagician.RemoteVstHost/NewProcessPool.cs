using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Catel;
using Catel.Collections;
using Catel.Data;
using Catel.Threading;
using PresetMagician.ProcessIsolation.Processes;
using Timer = System.Threading.Timer;

namespace PresetMagician.ProcessIsolation
{
    public class NewProcessPool : ObservableObject
    {
        private int _maxProcesses = 8;
        private int _maxStartTimeout = 20;
        private int _failedStartupProcesses;
        private int _totalStartedProcesses;

        public int NumRunningProcesses;
        public int NumTotalProcesses;
        
        public event EventHandler<PoolFailedEventArgs> PoolFailed;

        public FastObservableCollection<VstHostProcess> RunningProcesses { get; } =
            new FastObservableCollection<VstHostProcess>();

        public FastObservableCollection<VstHostProcess> OldProcesses { get; } =
            new FastObservableCollection<VstHostProcess>();

        private readonly Timer _processWatcher;
        public bool PoolRunning { get; private set; }

        private static readonly object _updateLock = new object();
        private bool _updateProcessesRunning;

        public NewProcessPool()
        {
            _processWatcher = new Timer(UpdateProcesses, null, 500, 500);

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                BindingOperations.EnableCollectionSynchronization(RunningProcesses, _updateLock);
                BindingOperations.EnableCollectionSynchronization(OldProcesses, _updateLock);
            }));
        }

        public void StartPool()
        {
            PoolRunning = true;
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
                _failedStartupProcesses = 0;
                PoolRunning = false;
                foreach (var process in RunningProcesses.ToArray())
                {
                    process.ForceStop("Pool shutdown");
                }
            }
        }


        private void UpdateProcesses(object o)
        {
            lock (_updateLock)
            {
                if (_updateProcessesRunning)
                {
                    return;
                }

                _updateProcessesRunning = true;

                var pluginsToRemove = new List<VstHostProcess>();
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

                if (PoolRunning)
                {
                    if (RunningProcesses.Count < _maxProcesses)
                    {
                        var process = new VstHostProcess(_maxStartTimeout);
                        process.ProcessStateUpdated += ProcessOnProcessStateUpdated;
                        TaskHelper.Run(() => process.Start());
                        RunningProcesses.Add(process);

                        _totalStartedProcesses++;
                    }
                }
                
                NumRunningProcesses = (from process in RunningProcesses
                    where process.CurrentProcessState == HostProcess.ProcessState.RUNNING
                    select process).Count();
                NumTotalProcesses = RunningProcesses.Count;


                _updateProcessesRunning = false;
            }

        }


        private void UnwatchProcess(VstHostProcess process)
        {
            process.ProcessStateUpdated -= ProcessOnProcessStateUpdated;
        }

        private void ProcessOnProcessStateUpdated(object sender, EventArgs e)
        {
            lock (_updateLock)
            {
                var process = sender as VstHostProcess;
                if (process.CurrentProcessState == HostProcess.ProcessState.EXITED && !process.StartupSuccessful &&
                    PoolRunning)
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
        }

      
    }

    public class PoolFailedEventArgs : EventArgs
    {
        public string ShutdownReason { get; set; }
    }
}