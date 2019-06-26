// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationInitializationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Anotar.Catel;
using Catel;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using MethodTimer;
using Orc.Scheduling;
using Orc.Squirrel;
using Orchestra.Services;
using PresetMagician.Core.Commands.Plugin;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Legacy.Services;
using PresetMagician.Legacy.Services.EventArgs;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;

namespace PresetMagician.Services
{
    public class ApplicationInitializationService : ApplicationInitializationServiceBase
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly IServiceLocator _serviceLocator;
        private readonly ICommandManager _commandManager;
        private readonly IUIVisualizerService _uiVisualizerService;
        private FrontendService _frontendService;
        private readonly SplashScreenService _splashScreenService;
        private SquirrelResult _squirrelResult;

        #endregion Fields

        #region Constructors

        public ApplicationInitializationService(IServiceLocator serviceLocator,
            ICommandManager commandManager, IUIVisualizerService uiVisualizerService,
            IViewModelFactory viewModelFactory)
        {
            Argument.IsNotNull(() => serviceLocator);
            Argument.IsNotNull(() => commandManager);
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => viewModelFactory);

            _serviceLocator = serviceLocator;
            _commandManager = commandManager;
            _uiVisualizerService = uiVisualizerService;

            _splashScreenService = serviceLocator.ResolveType<ISplashScreenService>() as SplashScreenService;


            _squirrelResult = new SquirrelResult();
        }

        #endregion Constructors

        #region Methods

        [Time]
        public override async Task InitializeBeforeCreatingShellAsync()
        {
            // Non-async first
            FrontendInitializer.RegisterTypes(_serviceLocator);
            FrontendInitializer.Initialize(_serviceLocator);
            _frontendService = _serviceLocator.ResolveType<FrontendService>();
            _splashScreenService.Action = "Loading configuration…";
            LoadConfiguration();


            _serviceLocator.ResolveType<IApplicationService>().StartProcessPool();

            _splashScreenService.Action = "Migrating database…";
            if (File.Exists(FileLocations.LegacyDatabasePath))
            {
                await TaskHelper.Run(() =>
                {
                    _serviceLocator.RegisterType<Ef6MigrationService, Ef6MigrationService>();
                    var migrationService = _serviceLocator.ResolveType<Ef6MigrationService>();
                    migrationService.MigrationProgressUpdated += delegate(object sender, MigrationProgessEventArgs args)
                    {
                        _splashScreenService.Action = "Migrating database…" + args.Progress;
                    };
                    Core.Core.UseDispatcher = false;
                    migrationService.LoadData();
                    migrationService.MigratePlugins();
                    Core.Core.UseDispatcher = true;
                }).ConfigureAwait(false);

                _serviceLocator.ResolveType<GlobalService>().RuntimeConfiguration.FileOverwriteMode =
                    PresetExportInfo.FileOverwriteMode.FORCE_OVERWRITE;
                _serviceLocator.ResolveType<GlobalService>().RuntimeConfiguration.FolderExportMode =
                    PresetExportInfo.FolderExportMode.ONE_LEVEL_LAST_BANK;
            }


            _splashScreenService.Action = "Initializing commands…";

            _frontendService.InitializeCommands();

            var dataPersistenceService = _serviceLocator.ResolveType<DataPersisterService>();
            var globalService = _serviceLocator.ResolveType<GlobalService>();
            var pluginFiles = dataPersistenceService.GetStoredPluginFiles();

            dataPersistenceService.LoadTypesCharacteristics();
            dataPersistenceService.LoadPreviewNotePlayers();

            foreach (var pluginFile in pluginFiles)
            {
                var plugin = dataPersistenceService.LoadPlugin(pluginFile);
                _splashScreenService.Action =
                    $"({pluginFiles.IndexOf(pluginFile) + 1}/{pluginFiles.Count}) Loading data for plugin {plugin.PluginName}";
                await dataPersistenceService.LoadPresetsForPlugin(plugin);

                globalService.Plugins.Add(plugin);
            }

            dataPersistenceService.LoadDontShowAgainDialogs();
            dataPersistenceService.LoadRememberMyChoiceResults();
        }

        [Time]
        public override async Task InitializeBeforeShowingShellAsync()
        {
            _splashScreenService.Action = "Almost there…";

            _frontendService.SetupCollectionSynchronizations();
        }

        [Time]
        public override async Task InitializeAfterShowingShellAsync()
        {
            LogTo.Debug("Running initialization after showing the shell");

            var serviceLocator = ServiceLocator.Default;
            var licenseService = serviceLocator.ResolveType<ILicenseService>();
            if (!licenseService.CheckLicense())
            {
                LogTo.Debug("No valid license found, showing registration dialog");
                await StartRegistration();
            }


            await base.InitializeAfterShowingShellAsync();

            LogTo.Debug("Scheduling update check task");
            var schedulerService = _serviceLocator.ResolveType<ISchedulingService>();

            var updateCheckTask = new ScheduledTask
            {
                Name = "Update Check task",
                Start = DateTime.Now.AddMinutes(1),
                Action = CheckForUpdatesAsync
            };

            schedulerService.AddScheduledTask(updateCheckTask);

            TaskHelper.Run(() => { _serviceLocator.ResolveType<RefreshPluginsCommand>().ExecuteAsync(); });

            var globalService = _serviceLocator.ResolveType<GlobalService>();

            var location = Assembly.GetExecutingAssembly().Location;
            var releaseNotesFile = Path.Combine(Path.GetDirectoryName(location), @"Resources\ReleaseNotes\",
                globalService.PresetMagicianVersion + ".txt");

            if (File.Exists(releaseNotesFile))
            {
                var ms = _serviceLocator.ResolveType<IAdvancedMessageService>();
                await ms.ShowOnceAsync(File.ReadAllText(releaseNotesFile),
                    "RELEASENOTES_" + globalService.PresetMagicianVersion, "Release Notes");
            }
        }

        private async Task StartRegistration()
        {
            await _uiVisualizerService.ShowDialogAsync<RegistrationViewModel>();
        }

        [Time]
        private async Task CheckForUpdatesAsync()
        {
            Log.Info("Checking for updates…");

            var updateService = _serviceLocator.ResolveType<IUpdateService>();


            _squirrelResult = await updateService.CheckForUpdatesAsync(new SquirrelContext());
        }

        public SquirrelResult getSquirrel()
        {
            return _squirrelResult;
        }


        [Time]
        private void LoadConfiguration()
        {
            var runtimeConfigurationService = _serviceLocator.ResolveType<IRuntimeConfigurationService>();
            runtimeConfigurationService.Load();
        }

        #endregion Methods
    }
}