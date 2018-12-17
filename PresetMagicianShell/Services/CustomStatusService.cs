using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Catel;
using Catel.Logging;
using Catel.Services;
using Fluent;
using Orchestra.Services;
using PresetMagicianShell.Services.Interfaces;

namespace PresetMagicianShell.Services
{
    class CustomStatusService: Orchestra.Services.StatusService, ICustomStatusService
    {
           #region Fields
        private readonly IStatusFilterService _statusFilterService;

        private StatusBarItem _statusBarItem;
        private IDispatcherService _dispatcherService;
        private string _lastStatus;
        #endregion

        #region Constructors
        public CustomStatusService(IStatusFilterService statusFilterService, IDispatcherService dispatcherService): base(statusFilterService)
        {
            Argument.IsNotNull(() => statusFilterService);
            Argument.IsNotNull(() => dispatcherService);

            _statusFilterService = statusFilterService;
            _dispatcherService = dispatcherService;

            var statusLogListener = new Orchestra.Logging.StatusLogListener(this);

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

        #region Methods
        public void Initialize(StatusBarItem statusBarItem)
        {
            Argument.IsNotNull(() => statusBarItem);

            _statusBarItem = statusBarItem;
        }

        public void Initialize(IStatusRepresenter statusRepresenter)
        {
          
        }
        private void OnResetTimerTick(object sender, System.EventArgs e)
        {
            var timer = (DispatcherTimer)sender;

            string finalStatus = (string)timer.Tag;

            timer.Stop();
            timer.Tick -= OnResetTimerTick;

            if (string.Equals(_lastStatus, finalStatus))
            {
                SetStatus("Ready");
            }
        }

        private void SetStatus(string status)
        {
            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusLines = status.Split(new[] { "\n", "\r\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                if (statusLines.Length > 0)
                {
                    status = statusLines[0];
                }
            }

            _dispatcherService.BeginInvoke(() =>
            {
                _statusBarItem.SetCurrentValue(Fluent.StatusBarItem.ValueProperty, status);
            });
        }
        #endregion
    }
}
