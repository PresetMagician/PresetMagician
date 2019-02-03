using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Orc.Squirrel;
using Orchestra;
using Orchestra.Services;
using Portable.Licensing;
using PresetMagician.Models;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.ViewModels
{
    public class StatusBarViewModel : ViewModelBase
    {
        #region Constructors

        public StatusBarViewModel(IUpdateService updateService,
            IApplicationInitializationService applicationInitializationService, ILicenseService licenseService,
            IRuntimeConfigurationService runtimeConfigurationService)
        {
            Argument.IsNotNull(() => updateService);
            Argument.IsNotNull(() => applicationInitializationService);
            Argument.IsNotNull(() => licenseService);
            Argument.IsNotNull(() => runtimeConfigurationService);

            _updateService = updateService;
            _applicationInitializationService = applicationInitializationService as ApplicationInitializationService;
            _licenseService = licenseService;

            _licenseService.LicenseChanged += OnLicenseChanged;
            ApplicationState = runtimeConfigurationService.ApplicationState;

            InstallUpdate = new TaskCommand(OnInstallUpdateExecute);
        }

        #endregion Constructors

        #region Fields

        private readonly IUpdateService _updateService;
        private readonly ApplicationInitializationService _applicationInitializationService;
        private readonly ILicenseService _licenseService;

        #endregion Fields

        #region Properties

        public bool IsUpdateAvailable { get; private set; }
        public bool IsUpdatedInstalled { get; private set; }
        public string UpdatedVersion { get; private set; }

        public string Version { get; private set; }
        public License CurrentLicense { get; set; }

        public ApplicationState ApplicationState { get; private set; }

        public TaskCommand InstallUpdate { get; set; }

        private async Task OnInstallUpdateExecute()
        {
            await _updateService.InstallAvailableUpdatesAsync(new SquirrelContext());
        }

        public string LicensedTo
        {
            get
            {
                if (_licenseService.GetCurrentLicense() != null)
                {
                    if (_licenseService.GetCurrentLicense().Type == LicenseType.Trial)
                    {
                        return $"Licensed to: {_licenseService.GetCurrentLicense().Customer.Name} " +
                               $"(Expires {_licenseService.GetCurrentLicense().Expiration.ToShortDateString()})";
                    }

                    return "Licensed to: " + _licenseService.GetCurrentLicense().Customer.Name;
                }

                return "Not Licensed";
            }
        }

        #endregion Properties

        #region Methods

        private void OnLicenseChanged(object sender, EventArgs e)
        {
            CurrentLicense = _licenseService.GetCurrentLicense();
            RaisePropertyChanged(nameof(LicensedTo));
        }

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _updateService.UpdateInstalled += OnUpdateInstalled;

            IsUpdateAvailable = _applicationInitializationService.getSquirrel().IsUpdateInstalledOrAvailable;
            UpdatedVersion = _applicationInitializationService.getSquirrel().NewVersion;
            Version = VersionHelper.GetCurrentVersion();
        }

        protected override async Task CloseAsync()
        {
            _updateService.UpdateInstalled -= OnUpdateInstalled;

            await base.CloseAsync();
        }

        private void OnUpdateInstalled(object sender, EventArgs e)
        {
            IsUpdatedInstalled = _updateService.IsUpdatedInstalled;
            IsUpdateAvailable = false;
        }

        #endregion Methods
    }
}