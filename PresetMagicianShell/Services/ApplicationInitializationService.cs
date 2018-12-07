// --------------------------------------------------------------------------------------------------------------------
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
using Catel.Runtime.Serialization.Json;
using Catel.Services;
using Catel.Threading;
using MethodTimer;
using Orc.Squirrel;
using Orchestra.Services;
using PresetMagicianShell.Services.Interfaces;
using PresetMagicianShell.ViewModels;

namespace PresetMagicianShell.Services
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
            LoadConfiguration();
            
            await TaskHelper.RunAndWaitAsync(new Func<Task>[]
            {
                CheckForUpdatesAsync
                
            });

            
        }
        
        public override async Task InitializeBeforeShowingShellAsync()
        {
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

            var y = updateService.CurrentChannel;
            Debug.WriteLine(y.DefaultUrl);

            Debug.WriteLine("IsUpdateSystemAvailable:" + updateService.IsUpdateSystemAvailable);
            Debug.WriteLine("CheckForUpdates:" + updateService.CheckForUpdates);

            Debug.WriteLine(y.IsPrerelease);
            _squirrelResult = await updateService.CheckForUpdatesAsync(new SquirrelContext());

#pragma warning disable 4014
            // Not dot await, it's a background thread
            //updateService.InstallAvailableUpdatesAsync(new SquirrelContext());
#pragma warning restore 4014
        }

        public SquirrelResult getSquirrel()
        {
            return _squirrelResult;
        }

        private void InitializeCommands()
        {
            Log.Info("Initializing commands");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Tools), "NksfView");
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
            serviceLocator.RegisterType<ILicenseService, LicenseService>();
            serviceLocator.RegisterTypeAndInstantiate<IRuntimeConfigurationService, RuntimeConfigurationService>();
        }

        #endregion Methods
    }
}