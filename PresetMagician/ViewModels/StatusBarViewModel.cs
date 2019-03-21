using System;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Orc.Squirrel;
using Orchestra.Services;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.ViewModels
{
    public class StatusBarViewModel : ViewModelBase
    {
        #region Constructors

        public StatusBarViewModel(IUpdateService updateService,
            IApplicationInitializationService applicationInitializationService,
            IRuntimeConfigurationService runtimeConfigurationService, GlobalService globalService,
            GlobalFrontendService globalFrontendService)
        {
            Argument.IsNotNull(() => updateService);
            Argument.IsNotNull(() => applicationInitializationService);
            Argument.IsNotNull(() => runtimeConfigurationService);

            _updateService = updateService;
            _applicationInitializationService = applicationInitializationService as ApplicationInitializationService;
            _globalService = globalService;
            ApplicationState = globalFrontendService.ApplicationState;

            InstallUpdate = new TaskCommand(OnInstallUpdateExecute);
        }

        #endregion Constructors

        #region Fields

        private readonly IUpdateService _updateService;
        private readonly ApplicationInitializationService _applicationInitializationService;
        private readonly GlobalService _globalService;

        #endregion Fields

        #region Properties

        public bool IsUpdateAvailable { get; private set; }
        public bool IsUpdatedInstalled { get; private set; }
        public string UpdatedVersion { get; private set; }

        public string Version { get; private set; }

        public ApplicationState ApplicationState { get; }

        public TaskCommand InstallUpdate { get; set; }

        private async Task OnInstallUpdateExecute()
        {
            await _updateService.InstallAvailableUpdatesAsync(new SquirrelContext());
        }

        #endregion Properties

        #region Methods

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _updateService.UpdateInstalled += OnUpdateInstalled;

            IsUpdateAvailable = _applicationInitializationService.getSquirrel().IsUpdateInstalledOrAvailable;
            UpdatedVersion = _applicationInitializationService.getSquirrel().NewVersion;
            Version = _globalService.PresetMagicianVersion;
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