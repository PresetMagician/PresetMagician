using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Services;

namespace PresetMagician.ViewModels
{
    public class RegistrationViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly ILicenseService _licenseService;
        private readonly ICommandManager _commandManager;

        public string SystemCode { get; }

        public event EventHandler LicenseUpdated;
        public bool ValidLicense => _licenseService.ValidLicense;

        public RegistrationViewModel(INavigationService navigationService, ILicenseService licenseService,
            ICommandManager commandManager)
        {
            Argument.IsNotNull(() => navigationService);
            Argument.IsNotNull(() => licenseService);
            Argument.IsNotNull(() => commandManager);

            _navigationService = navigationService;
            _licenseService = licenseService;
            _commandManager = commandManager;
            _licenseService.LicenseChanged += OnLicenseUpdated;

            CloseApplication = new TaskCommand(OnCloseApplicationExecuteAsync);
            GetLicense = new TaskCommand(OnGetLicenseExecuteAsync);
            SelectLicenseFile = new TaskCommand(OnSelectLicenseFileExecuteAsync);
            SystemCode = LicenseService.SystemCodeInfo.getSystemInfo();
        }

        private void OnLicenseUpdated(object o, EventArgs e)
        {
            LicenseUpdated.SafeInvoke(this);
        }

        public TaskCommand CloseApplication { get; private set; }

        private async Task OnCloseApplicationExecuteAsync()
        {
            _navigationService.CloseApplication();
        }

        public TaskCommand GetLicense { get; set; }

        private async Task OnGetLicenseExecuteAsync()
        {
            Process.Start(Settings.Links.GetTrialLicense);
        }

        public TaskCommand SelectLicenseFile { get; private set; }

        private async Task OnSelectLicenseFileExecuteAsync()
        {
            _commandManager.ExecuteCommand(Commands.Tools.UpdateLicense);
        }
    }
}