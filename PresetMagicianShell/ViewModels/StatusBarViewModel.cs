using Catel;
using Catel.MVVM;
using Orc.Squirrel;
using Orchestra;
using Orchestra.Services;
using PresetMagicianShell.Services;
using System;
using System.Threading.Tasks;

namespace PresetMagicianShell.ViewModels
{
    public class StatusBarViewModel : ViewModelBase

    {
        #region Fields

        private readonly IUpdateService _updateService;
        private readonly ApplicationInitializationService _applicationInitializationService;

        #endregion Fields

        #region Constructors

        public StatusBarViewModel(IUpdateService updateService, IApplicationInitializationService applicationInitializationService)
        {
            Argument.IsNotNull(() => updateService);
            Argument.IsNotNull(() => applicationInitializationService);

            _updateService = updateService;
            _applicationInitializationService = applicationInitializationService as ApplicationInitializationService;
        }

        #endregion Constructors

        #region Properties

        public bool IsUpdateAvailable { get; private set; }
        public bool IsUpdatedInstalled { get; private set; }
        public string UpdatedVersion { get; private set; }

        public string Version { get; private set; }

        #endregion Properties

        #region Methods

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