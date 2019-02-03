using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Models;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.ViewModels
{
    public class RegistrationViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly ICommandManager _commandManager;
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;

        public string SystemCode { get; }

        public bool ValidLicense => _runtimeConfigurationService.ApplicationState.ValidLicense;

        public RegistrationViewModel(INavigationService navigationService,
            IRuntimeConfigurationService runtimeConfigurationService,
            ICommandManager commandManager)
        {
            Argument.IsNotNull(() => navigationService);
            Argument.IsNotNull(() => runtimeConfigurationService);
            Argument.IsNotNull(() => commandManager);

            _navigationService = navigationService;
            _commandManager = commandManager;
            _runtimeConfigurationService = runtimeConfigurationService;
            
            _runtimeConfigurationService.ApplicationState.PropertyChanged += ApplicationStateOnPropertyChanged;

            CloseApplication = new TaskCommand(OnCloseApplicationExecuteAsync);
            GetLicense = new TaskCommand(OnGetLicenseExecuteAsync);
            SelectLicenseFile = new TaskCommand(OnSelectLicenseFileExecuteAsync);
            SystemCode = LicenseService.SystemCodeInfo.getSystemInfo();
        }

        private void ApplicationStateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ApplicationState.ValidLicense))
            {
                if (_runtimeConfigurationService.ApplicationState.ValidLicense)
                {
                    this.CancelAndCloseViewModelAsync().Wait();
                }
            }
        }

        public TaskCommand CloseApplication { get; }

        private async Task OnCloseApplicationExecuteAsync()
        {
            _navigationService.CloseApplication();
        }

        public TaskCommand GetLicense { get; set; }

        private async Task OnGetLicenseExecuteAsync()
        {
            Process.Start(Settings.Links.GetTrialLicense);
        }

        public TaskCommand SelectLicenseFile { get; }

        private async Task OnSelectLicenseFileExecuteAsync()
        {
            _commandManager.ExecuteCommand(Commands.Tools.UpdateLicense);
        }
    }
}