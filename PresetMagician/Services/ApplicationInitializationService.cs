// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationInitializationService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Anotar.Catel;
using Catel;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Runtime.Serialization;
using Catel.Runtime.Serialization.Xml;
using Catel.Services;
using Catel.Threading;
using MethodTimer;
using Orc.Scheduling;
using Orc.Squirrel;
using Orchestra.Services;
using PresetMagician.Serialization;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;
using SharedModels;

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
            _viewModelFactory = viewModelFactory;

            _splashScreenService = serviceLocator.ResolveType<ISplashScreenService>() as SplashScreenService;


            _squirrelResult = new SquirrelResult();
        }

        #endregion Constructors

        #region Methods

        [Time]
        public override async Task InitializeBeforeCreatingShellAsync()
        {
            // Non-async first
            RegisterTypes();

            ServiceLocator.Default.ResolveType<IApplicationService>().StartProcessPool();

            _splashScreenService.Action = "Initializing database…";
            InitDatabase();

            _splashScreenService.Action = "Loading configuration…";
            LoadConfiguration();

            _splashScreenService.Action = "Loading database…";
            InitializeCommands();

            
        }

        public override async Task InitializeBeforeShowingShellAsync()
        {
            await _serviceLocator.ResolveType<IVstService>().LoadPlugins(); 
            _splashScreenService.Action = "Almost there…";
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
                StartRegistration();
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

            TaskHelper.Run(() => { _commandManager.ExecuteCommand(Commands.Plugin.RefreshPlugins); });
        }

        private async void StartRegistration()
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
        private void InitializeCommands()
        {
            _commandManager.CreateCommandWithGesture(typeof(Commands.Application), "CancelOperation");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Application), "ClearLastOperationErrors");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Application), "ApplyConfiguration");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Application), "NotImplemented");

            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin),
                nameof(Commands.Plugin.LoadPluginsFromDatabase));
            
                
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "ScanPlugins");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin),
                nameof(Commands.Plugin.QuickScanPlugins));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin),
                nameof(Commands.Plugin.ScanSelectedPlugins));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin),
                nameof(Commands.Plugin.QuickScanSelectedPlugins));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin),
                nameof(Commands.Plugin.ScanSelectedPlugin));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "RefreshPlugins");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "AllToPresetExportList");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "SelectedToPresetExportList");
            
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin),
                nameof(Commands.Plugin.NotExportedAllToPresetExportList));
            
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin),
                nameof(Commands.Plugin.NotExportedSelectedToPresetExportList));
            
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "ReportUnsupportedPlugins");
            
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), nameof(Commands.Plugin.ForceReportPluginsToLive));
            
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), nameof(Commands.Plugin.ForceReportPluginsToDev));

            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "EnablePlugins");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "DisablePlugins");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools),
                nameof(Commands.PluginTools.ViewSettings));
            
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools),
                nameof(Commands.PluginTools.ViewPresets));
            
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools),
                nameof(Commands.PluginTools.ViewErrors));
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "ShowPluginInfo");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "ShowPluginEditor");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "ShowPluginChunk");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "LoadPlugin");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "UnloadPlugin");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools),
                nameof(Commands.PluginTools.ReportSinglePluginToLive));

           
            
            _commandManager.CreateCommandWithGesture(typeof(Commands.Preset), nameof(Commands.Preset.ApplyMidiNote));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Preset), nameof(Commands.Preset.RenamePresetBank));
            
            
            _commandManager.CreateCommandWithGesture(typeof(Commands.PresetExport), nameof(Commands.PresetExport.ClearSelected));
            _commandManager.CreateCommandWithGesture(typeof(Commands.PresetExport), nameof(Commands.PresetExport.ActivatePresetView));
            _commandManager.CreateCommandWithGesture(typeof(Commands.PresetExport), nameof(Commands.PresetExport.DoExport));
            _commandManager.CreateCommandWithGesture(typeof(Commands.PresetExport), nameof(Commands.PresetExport.ClearList));

            _commandManager.CreateCommandWithGesture(typeof(Commands.PresetTools),
                nameof(Commands.PresetTools.ShowPresetData));

            _commandManager.CreateCommandWithGesture(typeof(Commands.Tools), "NksfView");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Tools), "SettingsView");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Tools), nameof(Commands.Tools.UpdateLicense));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Tools), nameof(Commands.Tools.CompressDatabase));

            _commandManager.CreateCommandWithGesture(typeof(Commands.Help), nameof(Commands.Help.RequestSupport));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Help), nameof(Commands.Help.CreateBugReport));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Help), nameof(Commands.Help.CreateFeatureRequest));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Help), "OpenChatLink");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Help), "OpenDocumentationLink");
            
            _commandManager.CreateCommandWithGesture(typeof(Commands.Developer), nameof(Commands.Developer.SetCatelLogging));
        }

        [Time]
        private void LoadConfiguration()
        {
            var runtimeConfigurationService = _serviceLocator.ResolveType<IRuntimeConfigurationService>();
            runtimeConfigurationService.Load();
        }

        [Time]
        private void InitDatabase()
        {
            ApplicationDatabaseContext.InitializeViewCache();
        }
        
        private async Task InitializePerformanceAsync()
        {

            
            /*Catel.Windows.Controls.UserControl.DefaultCreateWarningAndErrorValidatorForViewModelValue = false;
            Catel.Windows.Controls.UserControl.DefaultSkipSearchingForInfoBarMessageControlValue = true;*/
        }

        [Time]
        private void RegisterTypes()
        {
            var serviceLocator = ServiceLocator.Default;
            ServiceLocator.Default.RegisterType<IXmlSerializer, TestSerializer>();
            serviceLocator.RegisterType<ISerializer, TestSerializer>();
            serviceLocator.RegisterType<IAboutInfoService, AboutInfoService>();
            serviceLocator.RegisterType<ICustomStatusService, CustomStatusService>();
            serviceLocator.RegisterType<IPleaseWaitService, CustomPleaseWaitService>();
            serviceLocator.RegisterType<ILicenseService, LicenseService>();
            serviceLocator.RegisterType<IRuntimeConfigurationService, RuntimeConfigurationService>();
            serviceLocator.RegisterType<IVstService, VstService>();
            serviceLocator.RegisterType<IApplicationService, ApplicationService>();
            serviceLocator.RegisterType<IDatabaseService, DatabaseService>();
            serviceLocator.RegisterType<IAdvancedMessageService, AdvancedMessageService>();
            serviceLocator
                .RegisterType<INativeInstrumentsResourceGeneratorService, NativeInstrumentsResourceGeneratorService>();
        }

        #endregion Methods
    }
}