// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationInitializationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading;
using Catel.Threading;
using MethodTimer;
using Orc.Squirrel;
using PresetMagicianShell.ViewModels;

namespace PresetMagicianShell.Services
{
    using Catel;
    using Catel.IoC;
    using Catel.Logging;
    using Catel.MVVM;
    using Orchestra.Services;
    using System.Threading.Tasks;

    public class ApplicationInitializationService : ApplicationInitializationServiceBase
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly IServiceLocator _serviceLocator;
        private readonly ICommandManager _commandManager;
        private readonly ITypeFactory _typeFactory;
        private readonly SplashScreenService _splashScreenService;

        #endregion Fields

        #region Constructors

        public ApplicationInitializationService(ITypeFactory typeFactory, IServiceLocator serviceLocator, ICommandManager commandManager)
        {
            Argument.IsNotNull(() => typeFactory);
            Argument.IsNotNull(() => serviceLocator);
            Argument.IsNotNull(() => commandManager);

            _typeFactory = typeFactory;
            _serviceLocator = serviceLocator;
            _commandManager = commandManager;

            _splashScreenService = serviceLocator.ResolveType<ISplashScreenService>() as SplashScreenService;
        }

        #endregion Constructors

        #region Methods

        public override async Task InitializeBeforeCreatingShellAsync()
        {
            // Non-async first
            RegisterTypes();
            InitializeCommands();

            await TaskHelper.RunAndWaitAsync(new Func<Task>[] {
                CheckForUpdatesAsync
            });

            _splashScreenService.Action = "Initialization complete.";
        }

        [Time]
        private async Task CheckForUpdatesAsync()
        {
            Log.Info("Checking for updates…");
            _splashScreenService.Action = "Checking for updates…";

            var updateService = _serviceLocator.ResolveType<IUpdateService>();
            updateService.Initialize(Settings.Application.AutomaticUpdates.AvailableChannels, Settings.Application.AutomaticUpdates.DefaultChannel,
                Settings.Application.AutomaticUpdates.CheckForUpdatesDefaultValue);

#pragma warning disable 4014
            // Not dot await, it's a background thread
            updateService.InstallAvailableUpdatesAsync(new SquirrelContext());
#pragma warning restore 4014
        }

        public override async Task InitializeAfterCreatingShellAsync()
        {
            await base.InitializeAfterCreatingShellAsync();
        }

        private void InitializeCommands()
        {
            Log.Info("Initializing commands");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Tools), "NksfView");

            var k = _commandManager.GetCommand("Tools.NksfView");
        }

        private async void RegisterTypes()
        {
            var serviceLocator = ServiceLocator.Default;
        }

        #endregion Methods
    }
}