using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anotar.Catel;
using Castle.DynamicProxy;
using Catel.Collections;
using Catel.Data;
using PresetMagician.Core.EventArgs;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.RemoteVstHost.Processes;

namespace PresetMagician.RemoteVstHost
{
    public class VstWorkerNotFoundException : Exception
    {
        public VstWorkerNotFoundException(string message) : base(message)
        {
        }
    }


    public class RemoteVstHostProcessPool : ObservableObject, IRemoteVstHostProcessPool, IDisposable
    {
        private int _maxProcesses = 4;
        private int _maxStartTimeout = 10;
        private int _failedStartupProcesses;
        private int _totalStartedProcesses;
        private int _minProcesses = 4;

        public int NumRunningProcesses { get; set; }
        public int NumTotalProcesses { get; set; }

        private IVstHostProcess _interactiveVstHostProcess;

        private readonly Dictionary<Plugin, IRemotePluginInstance> _interactivePluginInstances =
            new Dictionary<Plugin, IRemotePluginInstance>();

        private ProxyGenerator _generator = new ProxyGenerator();

        public event EventHandler<PoolFailedEventArgs> PoolFailed;


        public FastObservableCollection<IVstHostProcess> RunningProcesses { get; } =
            new FastObservableCollection<IVstHostProcess>();

        public FastObservableCollection<IVstHostProcess> OldProcesses { get; } =
            new FastObservableCollection<IVstHostProcess>();

        private readonly Timer _processWatcher;
        public bool PoolRunning { get; private set; }

        private static readonly object _updateLock = new object();
        private bool _updateProcessesRunning;

        public RemoteVstHostProcessPool()
        {
            _processWatcher = new Timer(UpdateProcesses, null, 500, 500);
        }

        public void StartPool()
        {
            PoolRunning = true;
        }

        public void SetMinProcesses(int minProcesses)
        {
            _minProcesses = minProcesses;
        }

        public void SetMaxProcesses(int maxProcesses)
        {
            if (maxProcesses >= _minProcesses)
            {
                _maxProcesses = maxProcesses;
            }
        }

        public IRemoteVstService GetVstService()
        {
            var type = typeof(IRemoteVstService);
            return _generator.CreateInterfaceProxyWithoutTarget(type, CreateInterceptor()) as IRemoteVstService;
        }

        protected virtual IInterceptor CreateInterceptor()
        {
            return new ProcessPoolInterceptor(this);
        }

        public void SetStartTimeout(int maxStartTimeoutSeconds)
        {
            if (_maxStartTimeout >= 10)
            {
                _maxStartTimeout = maxStartTimeoutSeconds;
            }
        }

        public IVstHostProcess GetFreeHostProcess()
        {
            for (var i = 0; i < _maxStartTimeout; i++)
            {
                var foundProcess = FindFreeHostProcess();
                if (foundProcess != null)
                {
                    return foundProcess;
                }

                if (i != 0)
                {
                    LogTo.Debug($"Warning: No free host process after {i} second(s)");
                }

                Thread.Sleep(1000);
            }

            throw new VstWorkerNotFoundException(
                $"Unable to find a free VST worker process within the maximum startup time of {_maxStartTimeout} seconds");
        }

        private IVstHostProcess FindFreeHostProcess()
        {
            return (from process in RunningProcesses
                where !process.IsBusy && process.CurrentProcessState == ProcessState.RUNNING
                select process).FirstOrDefault();
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

        public IRemotePluginInstance GetRemoteInteractivePluginInstance(Plugin plugin,
            bool backgroundProcessing = true)
        {
            if (_interactiveVstHostProcess == null)
            {
                _interactiveVstHostProcess = new VstHostProcess(20, true);
                _interactiveVstHostProcess.Start();
                _interactiveVstHostProcess.WaitUntilStarted();
            }

            if (!_interactivePluginInstances.ContainsKey(plugin))
            {
                _interactivePluginInstances.Add(plugin,
                    new RemotePluginInstance(_interactiveVstHostProcess, plugin, true, true));
            }

            return _interactivePluginInstances[plugin];
        }

        public IRemotePluginInstance GetRemotePluginInstance(Plugin plugin,
            bool backgroundProcessing = true)
        {
            var hostProcess = GetFreeHostProcess();
            return new RemotePluginInstance(hostProcess, plugin, backgroundProcessing);
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

                var processesToRemove = new List<IVstHostProcess>();
                foreach (var process in RunningProcesses)
                {
                    if (process.CurrentProcessState == ProcessState.EXITED)
                    {
                        UnwatchProcess(process);
                        processesToRemove.Add(process);
                        OldProcesses.Add(process);
                    }
                }

                foreach (var process in processesToRemove)
                {
                    RunningProcesses.Remove(process);
                }

                if (PoolRunning)
                {
                    if (RunningProcesses.Count < _maxProcesses)
                    {
                        var process = new VstHostProcess(_maxStartTimeout);
                        process.ProcessStateUpdated += ProcessOnProcessStateUpdated;
                        process.Start();
                        RunningProcesses.Add(process);

                        _totalStartedProcesses++;
                    }
                }

                NumRunningProcesses = (from process in RunningProcesses
                    where process.CurrentProcessState == ProcessState.RUNNING
                    select process).Count();
                NumTotalProcesses = RunningProcesses.Count;


                _updateProcessesRunning = false;
            }
        }


        private void UnwatchProcess(IVstHostProcess process)
        {
            process.ProcessStateUpdated -= ProcessOnProcessStateUpdated;
        }

        private void ProcessOnProcessStateUpdated(object sender, EventArgs e)
        {
            var process = sender as VstHostProcess;
            Debug.WriteLine("processstate is now " + process.CurrentProcessState);
            if (process.CurrentProcessState == ProcessState.EXITED && !process.StartupSuccessful &&
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
                    PoolFailed?.Invoke(this, eventArgs);
                }
            }
        }

        public void Dispose()
        {
            StopPool();
            _interactiveVstHostProcess?.ForceStop("Application Shutdown");
        }
    }
}