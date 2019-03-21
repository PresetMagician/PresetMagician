using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.ApplicationTask;
using PresetMagician.Core.EventArgs;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.RemoteVstHost;
using PresetMagician.Services.Interfaces;
using PresetMagician.Utils.Logger;
using PresetMagician.Utils.Logger.EventArgs;
using PresetMagician.Utils.Progress;
using Timer = System.Timers.Timer;

namespace PresetMagician.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly ICustomStatusService _statusService;
        private readonly IPleaseWaitService _pleaseWaitService;
        private readonly IAdvancedMessageService _messageService;
        private readonly DataPersisterService _dataPersisterService;
        private readonly IDispatcherService _dispatcherService;
        private readonly ApplicationOperationStatus _applicationOperationStatus;
        private string _lastUpdateStatus;
        private readonly Timer _updateWorkerPoolStatsTimer;
        private readonly Timer _updateDatabaseSizeTimer;
        private readonly GlobalService _globalService;
        private readonly GlobalFrontendService _globalFrontendService;
        public static bool UseDispatcher = true;
        private ApplicationProgress _applicationProgress;
        private ILog _log;


        private readonly List<string> _applicationOperationErrors = new List<string>();

        public ApplicationService(DataPersisterService dataPersisterService,
            IDispatcherService dispatcherService,
            ICustomStatusService statusService, IPleaseWaitService pleaseWaitService,
            IAdvancedMessageService messageService, GlobalService globalService,
            GlobalFrontendService globalFrontendService)
        {
            Argument.IsNotNull(() => statusService);
            Argument.IsNotNull(() => pleaseWaitService);
            Argument.IsNotNull(() => messageService);

            _pleaseWaitService = pleaseWaitService;
            _statusService = statusService;
            _messageService = messageService;
            _dispatcherService = dispatcherService;
            _dataPersisterService = dataPersisterService;
            _globalService = globalService;
            _globalFrontendService = globalFrontendService;
            _applicationOperationStatus = new ApplicationOperationStatus();

            _updateWorkerPoolStatsTimer = new Timer(500);
            _updateDatabaseSizeTimer = new Timer(2000);
        }

        public void Initialize()
        {
            _globalService.SetRemoteVstHostProcessPool(new RemoteVstHostProcessPool());
            _globalService.RemoteVstHostProcessPool.PoolFailed += NewProcessPoolOnPoolFailed;


            _updateWorkerPoolStatsTimer.Elapsed += UpdateWorkerPoolStatsTimerOnElapsed;
            _updateWorkerPoolStatsTimer.AutoReset = false;
            _updateWorkerPoolStatsTimer.Start();


            _updateDatabaseSizeTimer.Elapsed += UpdateDatabaseSizeTimerOnElapsed;
            _updateDatabaseSizeTimer.AutoReset = false;
            _updateDatabaseSizeTimer.Start();
        }

        private void UpdateDatabaseSizeTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _globalFrontendService.ApplicationState.DatabaseSize = _dataPersisterService.GetTotalDataSize();
            _updateDatabaseSizeTimer.Start();
        }

        private void UpdateWorkerPoolStatsTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _globalFrontendService.ApplicationState.RunningWorkers =
                _globalService.RemoteVstHostProcessPool.NumRunningProcesses;
            _globalFrontendService.ApplicationState.TotalWorkers =
                _globalService.RemoteVstHostProcessPool.NumTotalProcesses;
            _updateWorkerPoolStatsTimer.Start();
        }

        private void NewProcessPoolOnPoolFailed(object sender, PoolFailedEventArgs e)
        {
            _messageService.ShowErrorAsync(e.ShutdownReason, "VST worker pool failed",
                HelpLinks.CONCEPTS_VST_WORKER_POOL);
        }


        public void StartProcessPool()
        {
            _globalService.RemoteVstHostProcessPool.SetMaxProcesses(_globalService.RuntimeConfiguration.NumPoolWorkers);
            _globalService.RemoteVstHostProcessPool.SetStartTimeout(_globalService.RuntimeConfiguration
                .MaxPoolWorkerStartupTime);
            _globalService.RemoteVstHostProcessPool.StartPool();
        }

        public void ShutdownProcessPool()
        {
            _globalService.RemoteVstHostProcessPool.StopPool();
        }

        public CancellationTokenSource GetApplicationOperationCancellationSource()
        {
            return _applicationOperationStatus.ApplicationBusyCancellationTokenSource;
        }

        public ApplicationProgress GetApplicationProgress()
        {
            return _applicationProgress;
        }

        private void ClearApplicationProgress()
        {
            if (_applicationProgress != null)
            {
                ((Progress<CountProgress>) _applicationProgress.Progress).ProgressChanged -= OnProgressChanged;
                _applicationProgress.LogReporter.LogEntryAdded -= LogReporterOnLogEntryAdded;
                _applicationProgress = null;
            }
        }

        private void LogReporterOnLogEntryAdded(object sender, LogEntryAddedEventArgs e)
        {
            if (e.LogEntry.LogLevel == LogLevel.Error)
            {
                AddApplicationOperationError(e.LogEntry.Message);
            }
        }

        private void OnProgressChanged(object sender, CountProgress e)
        {
            UpdateApplicationOperationStatus(e.Current, e.Status);
        }

        public void StartApplicationOperation(object o, string operationDescription,
            int totalItems)
        {
            var appState = _globalFrontendService.ApplicationState;

            _log = LogManager.GetLogger(o.GetType());

            if (_applicationOperationStatus.IsApplicationBusy)
            {
                throw new ApplicationAlreadyBusyException(operationDescription);
            }


            _applicationOperationErrors.Clear();
            _applicationOperationStatus.ApplicationBusyCancellationTokenSource = new CancellationTokenSource();
            
            _applicationProgress = new ApplicationProgress
            {
                Progress = new Progress<CountProgress>(),
                LogReporter = new LogReporter(new MiniMemoryLogger()),
                CancellationToken = GetApplicationOperationCancellationSource().Token
            };
            ((Progress<CountProgress>) _applicationProgress.Progress).ProgressChanged += OnProgressChanged;
            _applicationProgress.LogReporter.LogEntryAdded += LogReporterOnLogEntryAdded;
            

            if (UseDispatcher)
            {
                _dispatcherService.Invoke(() =>
                {
                    _applicationOperationStatus.ApplicationOperationCancelRequested = false;
                    _applicationOperationStatus.ApplicationBusyTotalItems = totalItems;
                    _applicationOperationStatus.ApplicationBusyCurrentItem = 0;
                    _applicationOperationStatus.ApplicationBusyOperationDescription = operationDescription;
                    _applicationOperationStatus.ApplicationBusyStatusText = "";
                    _applicationOperationStatus.IsApplicationBusy = true;
                });
            }
            else
            {
                _applicationOperationStatus.ApplicationOperationCancelRequested = false;
                _applicationOperationStatus.ApplicationBusyTotalItems = totalItems;
                _applicationOperationStatus.ApplicationBusyCurrentItem = 0;
                _applicationOperationStatus.ApplicationBusyOperationDescription = operationDescription;
                _applicationOperationStatus.ApplicationBusyStatusText = "";
                _applicationOperationStatus.IsApplicationBusy = true;
            }

            appState.ApplyFromApplicationOperationStatus(_applicationOperationStatus);

            _log.Info($"Started operation \"{operationDescription}\"");
        }
        
        

  

        public void SetApplicationOperationTotalItems(int items)
        {
            if (UseDispatcher)
            {
                _dispatcherService.Invoke(() =>
                {
                    _applicationOperationStatus.ApplicationBusyTotalItems = items;
                });
            }
            else
            {
                _applicationOperationStatus.ApplicationBusyTotalItems = items;
            }
              

            _globalFrontendService.ApplicationState.ApplyFromApplicationOperationStatus(_applicationOperationStatus);
        }


        public List<string> GetApplicationOperationErrors()
        {
            return _applicationOperationErrors.ToList();
        }

        public void CancelApplicationOperation()
        {
            if (UseDispatcher)
            {
                _dispatcherService.Invoke(() =>
                {
                    _applicationOperationStatus.ApplicationBusyCancellationTokenSource.Cancel();
                    _applicationOperationStatus.ApplicationOperationCancelRequested = true;
                });
            }
            else
            {
                _applicationOperationStatus.ApplicationBusyCancellationTokenSource.Cancel();
                _applicationOperationStatus.ApplicationOperationCancelRequested = true;
            }

            _globalFrontendService.ApplicationState.ApplyFromApplicationOperationStatus(_applicationOperationStatus);

            _log.Info(
                $"Cancelling operation \"{_globalFrontendService.ApplicationState.ApplicationBusyOperationDescription}\"");
        }

        public void AddApplicationOperationError(string errorMessage)
        {
            _log.Error(errorMessage);
            _applicationOperationErrors.Add(errorMessage);
        }

        public void UpdateApplicationOperationStatus(int currentItem, string statusText)
        {
            var appState = _globalFrontendService.ApplicationState;
            var progressText = $"{currentItem} / {appState.ApplicationBusyTotalItems} {statusText}";

            if (UseDispatcher)
            {
                _dispatcherService.Invoke(() =>
                {
                    _applicationOperationStatus.ApplicationBusyStatusText = statusText;
                    _applicationOperationStatus.ApplicationBusyCurrentItem = currentItem;
                });
            }
            else
            {
                _applicationOperationStatus.ApplicationBusyStatusText = statusText;
                _applicationOperationStatus.ApplicationBusyCurrentItem = currentItem;
            }


            if (progressText == _lastUpdateStatus)
            {
                return;
            }

            _lastUpdateStatus = progressText;

            _pleaseWaitService.UpdateStatus(currentItem, appState.ApplicationBusyTotalItems);
            _statusService.UpdateStatus(progressText);
        }

        

        public void ReportStatus(string statusText)
        {
            _log.Info(statusText);
            _statusService.UpdateStatus(statusText);
        }

        public void ClearLastOperationErrors()
        {
            var appState = _globalFrontendService.ApplicationState;
            appState.ApplicationOperationLastOperation = null;
            appState.ApplicationOperationLastErrors.Clear();
            appState.ApplicationOperationLastErrorsAsText = null;
            appState.ApplicationOperationLastOperationHadErrors = false;
        }

        public void StopApplicationOperation(string finalMessage)
        {
            ClearApplicationProgress();
            var appState = _globalFrontendService.ApplicationState;

            if (UseDispatcher)
            {
                _dispatcherService.Invoke(() => { _applicationOperationStatus.IsApplicationBusy = false; });
            }
            else
            {
                _applicationOperationStatus.IsApplicationBusy = false;
            }

            appState.ApplyFromApplicationOperationStatus(_applicationOperationStatus);
            _pleaseWaitService.Hide();


            var lastErrors = GetApplicationOperationErrors();

            appState.ApplicationOperationLastOperation = appState.ApplicationBusyOperationDescription;
            appState.ApplicationOperationLastErrors = lastErrors;
            appState.ApplicationOperationLastErrorsAsText = String.Join(Environment.NewLine, lastErrors);
            appState.ApplicationOperationLastOperationHadErrors = lastErrors.Count > 0;

            string statusMessage;

            if (lastErrors.Count > 0)
            {
                statusMessage = $"{finalMessage} (with errors)";
                _log.Warning(
                    $"Finished operation \"{appState.ApplicationBusyOperationDescription}\" (with errors, see log)");
            }
            else
            {
                statusMessage = finalMessage;
                _log.Info($"Finished operation \"{appState.ApplicationBusyOperationDescription}\"");
            }

            _statusService.UpdateStatus(statusMessage);
        }

        public ApplicationOperationStatus GetApplicationOperationStatus()
        {
            return _applicationOperationStatus;
        }
    }
}