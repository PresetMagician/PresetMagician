using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Catel;
using Catel.IoC;
using Catel.Services;
using PresetMagicianShell.Services.Interfaces;
using PleaseWaitService = Orchestra.Services.PleaseWaitService;

namespace PresetMagicianShell.Services
{
    public class CustomPleaseWaitService : PleaseWaitService
    {
        private readonly IDependencyResolver _dependencyResolver;

        private readonly DispatcherTimer _hidingTimer;
        private readonly ICustomStatusService _statusService;
        private ProgressBar _progressBar;

        public CustomPleaseWaitService(IDispatcherService dispatcherService, IDependencyResolver dependencyResolver,
            ICustomStatusService statusService)
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

            _dispatcherService.BeginInvoke(() => { progressBar.Visibility = Visibility.Collapsed; });
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
                    progressBar.SetCurrentValue(RangeBase.MinimumProperty, (double) 0);
                    progressBar.SetCurrentValue(RangeBase.MaximumProperty, (double) totalItems);
                    progressBar.SetCurrentValue(RangeBase.ValueProperty, (double) currentItem);

                    if (currentItem < 0 || currentItem >= totalItems)
                    {
                        Hide();
                    }
                    else if (progressBar.Visibility != Visibility.Visible)
                    {
                        _hidingTimer.Stop();

                        progressBar.Visibility = Visibility.Visible;
                    }
                });
            }
        }

        private void OnHideTimerTick(object sender, System.EventArgs eventArgs)
        {
            _hidingTimer.Stop();

            var progressBar = InitializeProgressBar();
            if (progressBar == null)
            {
            }
        }

        private ProgressBar InitializeProgressBar()
        {
            if (_progressBar == null)
            {
                _progressBar = _dependencyResolver.TryResolve<ProgressBar>("pleaseWaitService");
            }

            return _progressBar;
        }
    }
}