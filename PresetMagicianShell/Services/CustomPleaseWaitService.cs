using Catel;
using Catel.Logging;
using Catel.Services;
using Orchestra.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Catel.IoC;
using PresetMagicianShell.Services.Interfaces;

namespace PresetMagicianShell.Services
{
    class CustomPleaseWaitService: Orchestra.Services.PleaseWaitService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IDependencyResolver _dependencyResolver;
        private ProgressBar _progressBar;
        private ICustomStatusService _statusService;

        private readonly DispatcherTimer _hidingTimer;

        public CustomPleaseWaitService(IDispatcherService dispatcherService, IDependencyResolver dependencyResolver, ICustomStatusService statusService)
            : base(dispatcherService)
        {
            Argument.IsNotNull(() => dependencyResolver);
            Argument.IsNotNull(() => statusService);

            _dependencyResolver = dependencyResolver;
            _statusService = statusService;

            _hidingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };

            _hidingTimer.Tick += OnHideTimerTick;
        }

        public override void Hide()
        {
            base.Hide();
            var progressBar = InitializeProgressBar();
            _statusService.UpdateStatus("");
            progressBar.Visibility = Visibility.Collapsed;

        }        

        public override void UpdateStatus(int currentItem, int totalItems, string statusFormat = "")
        {
            base.UpdateStatus(currentItem, totalItems, statusFormat);
            _statusService.UpdateStatus(statusFormat);
            var progressBar = InitializeProgressBar();
            if (progressBar != null)
            {
                _dispatcherService.BeginInvoke(() =>
                {
                    progressBar.SetCurrentValue(System.Windows.Controls.Primitives.RangeBase.MinimumProperty, (double)0);
                    progressBar.SetCurrentValue(System.Windows.Controls.Primitives.RangeBase.MaximumProperty, (double)totalItems);
                    progressBar.SetCurrentValue(System.Windows.Controls.Primitives.RangeBase.ValueProperty, (double)currentItem);

                    if (currentItem < 0 || currentItem >= totalItems)
                    {
                        Hide();
                    }
                    else if (progressBar.Visibility != Visibility.Visible)
                    {
                        Log.Debug("Showing progress bar");

                        _hidingTimer.Stop();

                        progressBar.Visibility = Visibility.Visible;
                    }                    
                }, true);
            }
        }

        private void OnHideTimerTick(object sender, System.EventArgs eventArgs)
        {
            Log.Debug("Hiding progress bar");

            _hidingTimer.Stop();

            var progressBar = InitializeProgressBar();
            if (progressBar == null)
            {
                return;
            }
        }

        private ProgressBar InitializeProgressBar()
        {
            if (_progressBar == null)
            {
                _progressBar = _dependencyResolver.TryResolve<ProgressBar>("pleaseWaitService");

                if (_progressBar != null)
                {
                    Log.Debug("Found progress bar that will represent progress inside the ProgressPleaseWaitService");
                }
            }

            return _progressBar;
        }

 }
}
