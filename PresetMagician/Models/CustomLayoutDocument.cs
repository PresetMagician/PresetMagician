using System.ComponentModel;
using System.Timers;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;
using Xceed.Wpf.AvalonDock.Layout;

namespace PresetMagician.Models
{
    public class CustomLayoutDocument : LayoutDocument
    {
        private readonly Timer _updateDirtyFlagTimer;

        public CustomLayoutDocument()
        {
            _updateDirtyFlagTimer = new Timer(500);
            _updateDirtyFlagTimer.Elapsed += UpdateDirtyFlagTimerOnElapsed;
            _updateDirtyFlagTimer.AutoReset = true;
            _updateDirtyFlagTimer.Start();
        }

        private void UpdateDirtyFlagTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (ShouldTrackModified && _viewModel is IModelTracker modelTracker)
            {
                IsDirty = modelTracker.GetTrackedModel().IsUserModified;
            }
        }


        protected override void OnClosed()
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
                _updateDirtyFlagTimer.Stop();
            }
        }

        protected override void OnClosing(CancelEventArgs args)
        {
            if (IsDirty)
            {
                var serviceLocator = ServiceLocator.Default;
                var messageService = serviceLocator.ResolveType<IAdvancedMessageService>();
                var messageResult = messageService.ShowAsync(
                        "You have unsaved changes. Do you wish to save? Use Cancel to continue editing.",
                        "Modifications Present", MessageButton.YesNoCancel, MessageImage.Question).ConfigureAwait(false)
                    .GetAwaiter().GetResult();

                switch (messageResult)
                {
                    case MessageResult.Cancel:
                        args.Cancel = true;
                        break;
                    case MessageResult.Yes:
                        _viewModel?.SaveViewModelAsync().ConfigureAwait(false);
                        break;
                    case MessageResult.No:
                        _viewModel?.CancelViewModelAsync().ConfigureAwait(false);
                        break;
                }
            }

            _viewModel?.CancelViewModelAsync().ConfigureAwait(false);
            _viewModel?.CloseViewModelAsync(null).ConfigureAwait(false);

            base.OnClosing(args);
        }

        private bool _isDirty;

        public bool IsDirty
        {
            get => _isDirty;
            private set
            {
                if (value == _isDirty)
                {
                    return;
                }

                _isDirty = value;
                RaisePropertyChanged(nameof(IsDirty));
            }
        }

        public bool ShouldTrackModified { private get; set; }

        private ViewModelBase _viewModel;

        public ViewModelBase ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
                }

                _viewModel = value;

                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
                }
            }
        }


        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModelBase.Title))
            {
                Title = ViewModel.Title;
            }
        }
    }
}