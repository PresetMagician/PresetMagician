using System;
using System.Windows.Threading;
using Catel;
using Catel.Logging;
using Catel.Services;
using Fluent;
using Orchestra.Services;
using PresetMagicianShell.Services.Interfaces;
using StatusLogListener = Orchestra.Logging.StatusLogListener;

namespace PresetMagicianShell.Services
{
    internal class CustomStatusService : StatusService, ICustomStatusService
    {
        #region Constructors

        public CustomStatusService(IStatusFilterService statusFilterService, IDispatcherService dispatcherService) :
            base(statusFilterService)
        {
            Argument.IsNotNull(() => statusFilterService);
            Argument.IsNotNull(() => dispatcherService);

            _statusFilterService = statusFilterService;
            _dispatcherService = dispatcherService;

            var statusLogListener = new StatusLogListener(this);

            LogManager.AddListener(statusLogListener);
        }

        #endregion

        #region IStatusService Members

        public new void UpdateStatus(string status)
        {
            var finalStatus = _statusFilterService.GetStatus(status);
            if (string.IsNullOrWhiteSpace(finalStatus))
            {
                return;
            }

            SetStatus(status);

            _lastStatus = finalStatus;
        }

        #endregion

        #region Fields

        private readonly IStatusFilterService _statusFilterService;

        private StatusBarItem _statusBarItem;
        private readonly IDispatcherService _dispatcherService;
        private string _lastStatus;

        #endregion

        #region Methods

        public void Initialize(StatusBarItem statusBarItem)
        {
            Argument.IsNotNull(() => statusBarItem);

            _statusBarItem = statusBarItem;
        }

        public void Initialize(IStatusRepresenter statusRepresenter)
        {
        }

        private void SetStatus(string status)
        {
            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusLines = status.Split(new[] {"\n", "\r\n", Environment.NewLine},
                    StringSplitOptions.RemoveEmptyEntries);
                if (statusLines.Length > 0)
                {
                    status = statusLines[0];
                }
            }

            _dispatcherService.BeginInvoke(() =>
            {
                _statusBarItem.SetCurrentValue(StatusBarItem.ValueProperty, status);
            });
        }

        #endregion
    }
}