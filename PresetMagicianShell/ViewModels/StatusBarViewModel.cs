using Catel;
using Catel.MVVM;
using Orc.Squirrel;
using Orchestra;
using Orchestra.Services;
using PresetMagicianShell.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Portable.Licensing;

namespace PresetMagicianShell.ViewModels
{
    public class StatusBarViewModel : ViewModelBase

    {
        #region Fields

        private readonly IUpdateService _updateService;
        private readonly ApplicationInitializationService _applicationInitializationService;
        private readonly ILicenseService _licenseService;

        #endregion Fields

        #region Constructors

        public StatusBarViewModel(IUpdateService updateService, IApplicationInitializationService applicationInitializationService, ILicenseService licenseService)
        {
            Argument.IsNotNull(() => updateService);
            Argument.IsNotNull(() => applicationInitializationService);
            Argument.IsNotNull(() => licenseService);

            _updateService = updateService;
            _applicationInitializationService = applicationInitializationService as ApplicationInitializationService;
            _licenseService = licenseService;

            _licenseService.LicenseChanged += OnLicenseChanged;
        }

        #endregion Constructors

        #region Properties

        public bool IsUpdateAvailable { get; private set; }
        public bool IsUpdatedInstalled { get; private set; }
        public string UpdatedVersion { get; private set; }

        public string Version { get; private set; }
        public License CurrentLicense { get; set; }


        public string LicensedTo
        {
            get
            {
                if (_licenseService.GetCurrentLicense() != null)
                {
                    return "Licensed to: "+_licenseService.GetCurrentLicense().Customer.Name;
                }
                else
                {
                    return "Not Licensed";
                }
            }
        }

        #endregion Properties

        #region Methods

        private void OnLicenseChanged(Object sender, EventArgs e)
        {
            CurrentLicense = _licenseService.GetCurrentLicense();
            RaisePropertyChanged("LicensedTo");

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
        }

        #endregion Methods
    }
}