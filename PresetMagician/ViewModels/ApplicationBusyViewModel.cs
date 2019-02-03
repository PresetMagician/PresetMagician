using System.ComponentModel;
using Catel.MVVM;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.ViewModels
{
    public class ApplicationBusyViewModel: ViewModelBase
    {
        public ApplicationState ApplicationState{ get; }
        
        public ApplicationBusyViewModel(IRuntimeConfigurationService configurationService)
        {
            ApplicationState = configurationService.ApplicationState;
            ApplicationState.PropertyChanged += ApplicationStateOnPropertyChanged;
        }

        private void ApplicationStateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ApplicationState.IsApplicationBusy))
            {
                if (ApplicationState.IsApplicationBusy == false)
                {
                    CloseAsync();
                }
            }
        }
    }
}