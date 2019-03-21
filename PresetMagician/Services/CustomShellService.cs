// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShellService.cs" company="WildGums">
//   Copyright (c) 2008 - 2014 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using System;
using System.Threading.Tasks;
using System.Windows;
using Catel;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Reflection;
using MethodTimer;
using Orchestra;
using Orchestra.Services;
using Orchestra.Views;
using AssemblyHelper = Catel.Reflection.AssemblyHelper;

namespace PresetMagician.Services
{
    public class CustomShellService : IShellService
    {
        #region Fields

        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly ITypeFactory _typeFactory;
        private readonly ICommandManager _commandManager;
        private readonly IKeyboardMappingsService _keyboardMappingsService;
        private readonly ISplashScreenService _splashScreenService;
        private readonly IApplicationInitializationService _applicationInitializationService;
        private readonly IDependencyResolver _dependencyResolver;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellService" /> class.
        /// </summary>
        /// <param name="typeFactory">The type factory.</param>
        /// <param name="keyboardMappingsService">The keyboard mappings service.</param>
        /// <param name="commandManager">The command manager.</param>
        /// <param name="splashScreenService">The splash screen service.</param>
        /// <param name="applicationInitializationService">The application initialization service.</param>
        /// <param name="dependencyResolver">The dependency resolver.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="typeFactory" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="keyboardMappingsService" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="commandManager" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="splashScreenService" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="applicationInitializationService" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="dependencyResolver" /> is <c>null</c>.</exception>
        public CustomShellService(ITypeFactory typeFactory, IKeyboardMappingsService keyboardMappingsService,
            ICommandManager commandManager,
            ISplashScreenService splashScreenService,
            IApplicationInitializationService applicationInitializationService, IDependencyResolver dependencyResolver)
        {
            Argument.IsNotNull(() => typeFactory);
            Argument.IsNotNull(() => keyboardMappingsService);
            Argument.IsNotNull(() => commandManager);
            Argument.IsNotNull(() => splashScreenService);
            Argument.IsNotNull(() => applicationInitializationService);
            Argument.IsNotNull(() => dependencyResolver);

            _typeFactory = typeFactory;
            _keyboardMappingsService = keyboardMappingsService;
            _commandManager = commandManager;
            _splashScreenService = splashScreenService;
            _applicationInitializationService = applicationInitializationService;
            _dependencyResolver = dependencyResolver;

            var entryAssembly = AssemblyHelper.GetEntryAssembly();

            Log.Info("Starting {0} v{1} ({2})", entryAssembly.Title(), entryAssembly.Version(),
                entryAssembly.InformationalVersion());

            // Initialize (now we have an application)
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the shell.
        /// </summary>
        /// <value>The shell.</value>
        public IShell Shell { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new shell and shows a splash during the initialization.
        /// </summary>
        /// <typeparam name="TShell">The type of the shell.</typeparam>
        /// <returns>The created shell.</returns>
        /// <exception cref="OrchestraException">The shell is already created and cannot be created again.</exception>
        [Time]
        public Task<TShell> CreateWithSplashAsync<TShell>()
            where TShell : class, IShell
        {
            return CreateAsync<TShell>();
        }


        /// <summary>
        /// Creates a new shell.
        /// </summary>
        /// <typeparam name="TShell">The type of the shell.</typeparam>
        /// <returns>The created shell.</returns>
        /// <exception cref="OrchestraException">The shell is already created and cannot be created again.</exception>
        [Time]
        public async Task<TShell> CreateAsync<TShell>()
            where TShell : class, IShell
        {
            await _applicationInitializationService.InitializeBeforeShowingSplashScreenAsync();

            TShell shell = null;

            if (_applicationInitializationService.ShowSplashScreen)
            {
                Log.Debug("Showing splash screen");

                var splashScreen = _splashScreenService.CreateSplashScreen();
                splashScreen.Show();

                shell = await CreateShellInternalAsync<TShell>(splashScreen.Close);
            }
            else
            {
                Log.Debug("Not showing splash screen");

                // Note: it's important to change the application mode. If we are not showing a splash screen,
                // the app won't have a window and will immediately close (if we start any task that is awaited)
                var application = Application.Current;
                var currentApplicationCloseMode = application.ShutdownMode;
                application.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                shell = await CreateShellInternalAsync<TShell>();

                application.ShutdownMode = currentApplicationCloseMode;
            }

            return shell;
        }

        /// <summary>
        /// Creates a new shell.
        /// </summary>
        /// <typeparam name="TShell">The type of the shell.</typeparam>
        /// <param name="postShowShellCallback">The shell created callback.</param>
        /// <returns>The created shell.</returns>
        /// <exception cref="OrchestraException">The shell is already created and cannot be created again.</exception>
        private async Task<TShell> CreateShellInternalAsync<TShell>(Action postShowShellCallback = null)
            where TShell : IShell
        {
            if (Shell != null)
            {
                throw Log.ErrorAndCreateException<OrchestraException>(
                    "The shell is already created and cannot be created again");
            }

            Log.Info("Checking if software was correctly closed previously");

            var shell = default(TShell);
            var successfullyStarted = true;

#if !DEBUG
            try
            {
#endif
            await InitializeBeforeCreatingShellAsync();

            shell = await CreateShellAsync<TShell>();

            _keyboardMappingsService.Load();

            // Now we have a new window, resubscribe the command manager
            _commandManager.SubscribeToKeyboardEvents();

            await InitializeAfterCreatingShellAsync();

            Log.Info("Confirming that application was started successfully");

            await InitializeBeforeShowingShellAsync();

            ShowShell(shell);

            if (postShowShellCallback != null)
            {
                postShowShellCallback();
            }

            await InitializeAfterShowingShellAsync();
#if !DEBUG
            }
            catch (Exception ex)
            {
                
                
                App.ReportCrash(ex);
            }
#endif
            return shell;
        }

        [Time]
        private async Task InitializeBeforeCreatingShellAsync()
        {
            Log.Debug("Calling IApplicationInitializationService.InitializeBeforeCreatingShell");

            await _applicationInitializationService.InitializeBeforeCreatingShellAsync();
        }

        [Time]
        private async Task InitializeAfterCreatingShellAsync()
        {
            Log.Debug("Calling IApplicationInitializationService.InitializeAfterCreatingShell");

            await _applicationInitializationService.InitializeAfterCreatingShellAsync();
        }

        void OnCreatingShell()
        {
            FluentRibbonHelper.ApplyTheme();
        }

        [Time]
        private async Task<TShell> CreateShellAsync<TShell>()
            where TShell : IShell
        {
            Log.Debug("Creating shell using type '{0}'", typeof(TShell).GetSafeFullName(false));

            OnCreatingShell();

            var shell = _typeFactory.CreateInstance<TShell>();
            Shell = shell;

            var shellAsWindow = Shell as Window;
            if (shellAsWindow != null)
            {
                Log.Debug("Setting the new shell as Application.MainWindow");

                shellAsWindow.Owner = null;

                var currentApp = Application.Current;
                currentApp.MainWindow = shellAsWindow;
            }


            return shell;
        }


        [Time]
        private void ShowShell(IShell shell)
        {
            if (!_applicationInitializationService.ShowShell)
            {
                Log.Debug("Not showing shell");
                return;
            }

            Log.Debug("Showing shell");

            shell.Show();
        }

        [Time]
        private async Task InitializeBeforeShowingShellAsync()
        {
            Log.Debug("Calling IApplicationInitializationService.InitializeBeforeShowingShell");

            await _applicationInitializationService.InitializeBeforeShowingShellAsync();
        }

        [Time]
        private async Task InitializeAfterShowingShellAsync()
        {
            Log.Debug("Calling IApplicationInitializationService.InitializeAfterShowingShell");

            await _applicationInitializationService.InitializeAfterShowingShellAsync();
        }

        #endregion
    }
}