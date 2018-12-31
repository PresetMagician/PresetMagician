﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationInitializationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Catel;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using MethodTimer;
using Orc.Squirrel;
using Orchestra.Services;
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
        private readonly IViewModelFactory _viewModelFactory;
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly SplashScreenService _splashScreenService;
        private SquirrelResult _squirrelResult;

        #endregion Fields

        #region Constructors

        public ApplicationInitializationService(IServiceLocator serviceLocator,
            ICommandManager commandManager, IUIVisualizerService uiVisualizerService, IViewModelFactory viewModelFactory)
        {
            Argument.IsNotNull(() => serviceLocator);
            Argument.IsNotNull(() => commandManager);
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => viewModelFactory);

            _serviceLocator = serviceLocator;
            _commandManager = commandManager;
            _uiVisualizerService = uiVisualizerService;
            _viewModelFactory = viewModelFactory;

            _splashScreenService = serviceLocator.ResolveType<ISplashScreenService>() as SplashScreenService;
            _squirrelResult = new SquirrelResult();

        }

        #endregion Constructors

        #region Methods

        public override async Task InitializeBeforeCreatingShellAsync()
        {
            // Non-async first
            RegisterTypes();
            InitializeCommands();
            
            await TaskHelper.RunAndWaitAsync(new Func<Task>[]
            {
                CheckForUpdatesAsync
                
            });

            
        }
        
        public override async Task InitializeBeforeShowingShellAsync()
        {
            _splashScreenService.Action = "Loading configuration…";
            LoadConfiguration();
            _splashScreenService.Action = "Restoring application layout…";
            var x = _serviceLocator.ResolveType<IRuntimeConfigurationService>();
            x.LoadLayout();
        }

        public override Task InitializeAfterShowingShellAsync()
        {
            var serviceLocator = ServiceLocator.Default;
            var licenseService = serviceLocator.ResolveType<ILicenseService>(); 
            if (!licenseService.CheckLicense()) {
                StartRegistration();
            }
            return base.InitializeAfterShowingShellAsync();
        }

        private async void StartRegistration()
        {
            var viewModel = _viewModelFactory.CreateViewModel(typeof(RegistrationViewModel), null);

            await _uiVisualizerService.ShowDialogAsync(viewModel);
        }

        [Time]
        private async Task CheckForUpdatesAsync()
        {
            Log.Info("Checking for updates…");
            _splashScreenService.Action = "Checking for updates…";

            var updateService = _serviceLocator.ResolveType<IUpdateService>();
            updateService.Initialize(Settings.Application.AutomaticUpdates.AvailableChannels,
                Settings.Application.AutomaticUpdates.DefaultChannel,
                Settings.Application.AutomaticUpdates.CheckForUpdatesDefaultValue);

            _squirrelResult = await updateService.CheckForUpdatesAsync(new SquirrelContext());
            
        }

        public SquirrelResult getSquirrel()
        {
            return _squirrelResult;
        }

        private void InitializeCommands()
        {
            _commandManager.CreateCommandWithGesture(typeof(Commands.Application), "CancelOperation");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Application), "ClearLastOperationErrors");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Application), "ApplyConfiguration");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Application), "NotImplemented");
            
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "ScanPlugins");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "RefreshPlugins");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "AllToPresetExportList");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "SelectedToPresetExportList");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "ReportUnsupportedPlugins");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "ReportAllPlugins");

            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "EnablePlugins");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "DisablePlugins");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "ShowPluginInfo");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "ShowPluginEditor");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "ShowPluginChunk");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "LoadPlugin");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "UnloadPlugin");

            _commandManager.CreateCommandWithGesture(typeof(Commands.Preset), "ActivatePresetView");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Preset), "Export");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Preset), "ClearList");
            
            _commandManager.CreateCommandWithGesture(typeof(Commands.PresetTools), nameof(Commands.PresetTools.ShowPresetData));

            _commandManager.CreateCommandWithGesture(typeof(Commands.Tools), "NksfView");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Tools), "SettingsView");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Tools), nameof(Commands.Tools.UpdateLicense));
            
            _commandManager.CreateCommandWithGesture(typeof(Commands.Help), "OpenSupportLink");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Help), "OpenChatLink");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Help), "OpenDocumentationLink");

            
        }
        
        private void LoadConfiguration()
        {
            var runtimeConfigurationService = _serviceLocator.ResolveType<IRuntimeConfigurationService>();
            runtimeConfigurationService.Load();
        }

        private void RegisterTypes()
        {
            var serviceLocator = ServiceLocator.Default;
            serviceLocator.RegisterType<IAboutInfoService, AboutInfoService>();
            serviceLocator.RegisterType<ICustomStatusService, CustomStatusService>();
            serviceLocator.RegisterType<IPleaseWaitService, CustomPleaseWaitService>();
            serviceLocator.RegisterType<ILicenseService, LicenseService>();
            serviceLocator.RegisterType<IRuntimeConfigurationService, RuntimeConfigurationService>();
            serviceLocator.RegisterType<IVstService, VstService>();
            serviceLocator.RegisterType<IApplicationService, ApplicationService>();
        }

        #endregion Methods
    }
}