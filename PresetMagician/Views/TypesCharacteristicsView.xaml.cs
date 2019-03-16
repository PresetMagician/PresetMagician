using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Fluent;
using Orchestra;
using PresetMagician.ViewModels;

namespace PresetMagician.Views
{
    public partial class TypesCharacteristicsView
    {
        private ProgressWindowViewModel _closingViewModel;
        private IUIVisualizerService _visualizerService;
        
        public TypesCharacteristicsView()
        {
            _visualizerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();
            InitializeComponent();

        }

        protected override void OnViewModelChanged()
        {
            base.OnViewModelChanged();
            if (ViewModel != null)
            {
                ViewModel.CancelingAsync += ViewModelOnCancelingAsync;
                ViewModel.CanceledAsync += ViewModelOnCanceledAsync;
                ViewModel.SavingAsync += ViewModelOnSavingAsync;
                ViewModel.SavedAsync += ViewModelOnSavedAsync;
            }
        }


        private void RemoveEventListeners()
        {
            ViewModel.CancelingAsync -= ViewModelOnCancelingAsync;
            ViewModel.CanceledAsync -= ViewModelOnCanceledAsync;
            ViewModel.SavingAsync -= ViewModelOnSavingAsync;
            ViewModel.SavedAsync -= ViewModelOnSavedAsync;
        }

        private async Task ViewModelOnCanceledAsync(object sender, EventArgs e)
        {
            await _closingViewModel.CancelAndCloseViewModelAsync();
            RemoveEventListeners();
        }

        private async Task ViewModelOnSavedAsync(object sender, EventArgs e)
        {
            await _closingViewModel.CancelAndCloseViewModelAsync();
            RemoveEventListeners();
        }

        private async Task ViewModelOnSavingAsync(object sender, SavingEventArgs e)
        {
            _closingViewModel= new ProgressWindowViewModel();
            _closingViewModel.Title = "Saving...";
            _closingViewModel.SetOwnerWindow(this);
            _visualizerService.ShowDialogAsync(_closingViewModel);
        }

        private async Task ViewModelOnCancelingAsync(object sender, CancelingEventArgs e)
        {
            _closingViewModel= new ProgressWindowViewModel();
            _closingViewModel.Title = "Cancelling...";
            _closingViewModel.SetOwnerWindow(this);
            _visualizerService.ShowDialogAsync(_closingViewModel);

        }
    }
}