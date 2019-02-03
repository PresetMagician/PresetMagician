using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.ProcessIsolation;
using PresetMagician.Services.Interfaces;
using Timer = System.Timers.Timer;

namespace PresetMagician.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private readonly ICustomStatusService _statusService;
        private readonly IPleaseWaitService _pleaseWaitService;
        private readonly IAdvancedMessageService _messageService;
        private readonly IDatabaseService _databaseService;
        private string _lastUpdateStatus;
        private readonly Timer _updateWorkerPoolStatsTimer;
        private readonly Timer _updateDatabaseSizeTimer;
        private ILog _log;
        public NewProcessPool NewProcessPool { get; }

        private readonly List<string> _applicationOperationErrors = new List<string>();

        public ApplicationService(IRuntimeConfigurationService runtimeConfigurationService,
            ICustomStatusService statusService, IPleaseWaitService pleaseWaitService, IAdvancedMessageService messageService, IDatabaseService databaseService)
        {
            Argument.IsNotNull(() => runtimeConfigurationService);
            Argument.IsNotNull(() => statusService);
            Argument.IsNotNull(() => pleaseWaitService);
            Argument.IsNotNull(() => messageService);
            Argument.IsNotNull(() => databaseService);

            _pleaseWaitService = pleaseWaitService;
            _statusService = statusService;
            _runtimeConfigurationService = runtimeConfigurationService;
            _messageService = messageService;
            _databaseService = databaseService;
            
          
            NewProcessPool = new NewProcessPool();
            NewProcessPool.PoolFailed += NewProcessPoolOnPoolFailed;
            
            _updateWorkerPoolStatsTimer = new Timer(500);
            _updateWorkerPoolStatsTimer.Elapsed += UpdateWorkerPoolStatsTimerOnElapsed;
            _updateWorkerPoolStatsTimer.AutoReset = false;
            _updateWorkerPoolStatsTimer.Start();
            
            _updateDatabaseSizeTimer = new Timer(2000);
            _updateDatabaseSizeTimer.Elapsed += UpdateDatabaseSizeTimerOnElapsed;
            _updateDatabaseSizeTimer.AutoReset = false;
            _updateDatabaseSizeTimer.Start();

        }

        private void UpdateDatabaseSizeTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _databaseService.UpdateDatabaseSize();
            _updateDatabaseSizeTimer.Start();
        }

        private void UpdateWorkerPoolStatsTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _runtimeConfigurationService.ApplicationState.RunningWorkers = NewProcessPool.NumRunningProcesses;
            _runtimeConfigurationService.ApplicationState.TotalWorkers = NewProcessPool.NumTotalProcesses;
            _updateWorkerPoolStatsTimer.Start();
        }

        private void NewProcessPoolOnPoolFailed(object sender, PoolFailedEventArgs e)
        {
            _messageService.ShowErrorAsync(e.ShutdownReason, "VST worker pool failed", HelpLinks.CONCEPTS_VST_WORKER_POOL);
        }
        
      
        public void StartProcessPool()
        {
            NewProcessPool.SetMaxProcesses(_runtimeConfigurationService.RuntimeConfiguration.NumPoolWorkers);
            NewProcessPool.SetStartTimeout(_runtimeConfigurationService.RuntimeConfiguration.MaxPoolWorkerStartupTime);
            NewProcessPool.StartPool();
        }

        public void ShutdownProcessPool()
        {
            NewProcessPool.StopPool();
        }

        public CancellationTokenSource GetApplicationOperationCancellationSource()
        {
            var appState = _runtimeConfigurationService.ApplicationState;
            return appState.ApplicationBusyCancellationTokenSource;
        }

        public void StartApplicationOperation(CommandContainerBase commandContainer, string operationDescription,
            int totalItems)
        {
            var sanitizedCommandName = commandContainer.CommandName.Replace(".", "");
            var propertyName = $"Is{sanitizedCommandName}Running";

            StartApplicationOperation(commandContainer, propertyName, operationDescription, totalItems);
        }

        public void StartApplicationOperation(object o, string operationDescription,
            int totalItems)
        {
            StartApplicationOperation(o, null, operationDescription, totalItems);
        }

        public void StartApplicationOperation(object o, string targetPropertyName, string operationDescription,
            int totalItems)
        {
            var appState = _runtimeConfigurationService.ApplicationState;

            _log = LogManager.GetLogger(o.GetType());

            if (appState.IsApplicationBusy)
            {
                throw new ApplicationAlreadyBusyException(operationDescription);
            }

            appState.ApplicationOperationSourceObject = o;
            appState.ApplicationOperationStatePropertyName = targetPropertyName;

            if (targetPropertyName != null)
            {
                //SetApplicationStateProperty(true);
            }

            _applicationOperationErrors.Clear();
            appState.ApplicationOperationCancelRequested = false;
            appState.ApplicationBusyCancellationTokenSource = new CancellationTokenSource();
            appState.ApplicationBusyTotalItems = totalItems;
            appState.ApplicationBusyCurrentItem = 0;
            appState.ApplicationBusyOperationDescription = operationDescription;
            appState.ApplicationBusyStatusText = "";
            appState.IsApplicationBusy = true;

            _log.Info($"Started operation \"{operationDescription}\"");
        }

        public void SetApplicationOperationTotalItems(int items)
        {
            _runtimeConfigurationService.ApplicationState.ApplicationBusyTotalItems = items;
        }

        private void SetApplicationStateProperty(object value)
        {
            var appState = _runtimeConfigurationService.ApplicationState;
            try
            {
                var property = appState.GetType().GetProperty(appState.ApplicationOperationStatePropertyName);
                property.SetValue(_runtimeConfigurationService.ApplicationState, value);
            }
            catch (Exception e) when (e is ArgumentNullException || e is NullReferenceException)
            {
                throw new ArgumentException(
                    $"Property {appState.ApplicationOperationStatePropertyName} is not defined or not accessible on " +
                    $"{appState.GetType().FullName}, but was passed by targetPropertyName. " +
                    "Maybe you relied on the CommandContainer auto-wiring and forgot to implement it.",
                    e);
            }
        }

        public List<string> GetApplicationOperationErrors()
        {
            return _applicationOperationErrors.ToList();
        }

        public void CancelApplicationOperation()
        {
            var appState = _runtimeConfigurationService.ApplicationState;
            appState.ApplicationBusyCancellationTokenSource.Cancel();
            appState.ApplicationOperationCancelRequested = true;
            _log.Info($"Cancelling operation \"{appState.ApplicationBusyOperationDescription}\"");
        }

        public void AddApplicationOperationError(string errorMessage)
        {
            _log.Error(errorMessage);
            _applicationOperationErrors.Add(errorMessage);
        }

        public void UpdateApplicationOperationStatus(int currentItem, string statusText)
        {
            var appState = _runtimeConfigurationService.ApplicationState;
            appState.ApplicationBusyStatusText = statusText;

            var progressText = string.Format("({1} / {2}) {0}", statusText, currentItem,
                appState.ApplicationBusyTotalItems);
            appState.ApplicationBusyCurrentItem = currentItem;

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
            var appState = _runtimeConfigurationService.ApplicationState;
            appState.ApplicationOperationLastOperation = null;
            appState.ApplicationOperationLastErrors.Clear();
            appState.ApplicationOperationLastErrorsAsText = null;
            appState.ApplicationOperationLastOperationHadErrors = false;
        }

        public void StopApplicationOperation(string finalMessage)
        {
            var appState = _runtimeConfigurationService.ApplicationState;
            appState.IsApplicationBusy = false;
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
    }
}