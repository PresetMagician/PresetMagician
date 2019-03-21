using System;
using System.IO;
using System.Threading;
using Catel;
using Catel.IoC;
using Catel.Logging;
using Catel.Services;
using Orchestra.Collections;
using Orchestra.Layers;
using Orchestra.Services;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Services;
using PresetMagician.Legacy;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using MessageService = Orchestra.Services.MessageService;
using PleaseWaitService = Orchestra.Services.PleaseWaitService;
using SplashScreenService = Orchestra.Services.SplashScreenService;

namespace PresetMagician.Tests
{
    public class DataFixture : IDisposable
    {
        private string _className;
        private ServiceLocator _serviceLocator;

        public DataFixture()
        {
            _serviceLocator = new ServiceLocator();
        }

        public ServiceLocator GetServiceLocator()
        {
            return _serviceLocator;
        }

        public void Setup(string className)
        {
            EditableCollectionConfiguration.DisableDispatch = true;
            ApplicationService.UseDispatcher = false;
            
            LogManager.AddDebugListener(false);
            var module2 = new CoreModule();
            module2.Initialize(_serviceLocator);
            var module = new MVVMModule();
            module.Initialize(_serviceLocator);
            RegisterOrchestraTypes();

            ApplicationDatabaseContext.DefaultDatabasePath = Path.Combine(Directory.GetCurrentDirectory(),
                $@"TestData\{className}\LegacyDatabases\LegacyDb.sqlite3");
            DataPersisterService.DefaultPluginStoragePath =
                Path.Combine(Directory.GetCurrentDirectory(), $@"TestData\{className}\Plugins");
            DataPersisterService.DefaultTypesCharacteristicsStoragePath =
                Path.Combine(Directory.GetCurrentDirectory(), $@"TestData\{className}");
            PresetDataPersisterService.DefaultDatabasePath = Path.Combine(Directory.GetCurrentDirectory(),
                $@"TestData\{className}\PresetData.sqlite3");

            Directory.CreateDirectory(DataPersisterService.DefaultPluginStoragePath);
            Directory.CreateDirectory(Path.GetDirectoryName(ApplicationDatabaseContext.DefaultDatabasePath));
            Directory.CreateDirectory(Path.GetDirectoryName(PresetDataPersisterService.DefaultDatabasePath));


            File.Delete(ApplicationDatabaseContext.DefaultDatabasePath);
            File.Copy(@"Resources\PresetMagician.test.sqlite3", ApplicationDatabaseContext.DefaultDatabasePath);

            FrontendInitializer.RegisterTypes(_serviceLocator);
            FrontendInitializer.Initialize(_serviceLocator);

            var frontendService = _serviceLocator.ResolveType<FrontendService>();
            frontendService.InitializeCommands();
        }

        public void StartPool()
        {
            var applicationService = _serviceLocator.ResolveType<IApplicationService>();
            var globalService = _serviceLocator.ResolveType<GlobalService>();
            globalService.RemoteVstHostProcessPool.SetMinProcesses(1);

            applicationService.StartProcessPool();


            while (globalService.RemoteVstHostProcessPool.RunningProcesses.Count != 1)
            {
                Thread.Sleep(50);
            }
        }

        public void StopPool()
        {
            var applicationService = _serviceLocator.ResolveType<IApplicationService>();
            applicationService.ShutdownProcessPool();
        }

        private void RegisterOrchestraTypes()
        {
            var serviceLocator = _serviceLocator;
            serviceLocator.RegisterType<IPleaseWaitService, PleaseWaitService>();

            serviceLocator.RegisterTypeIfNotYetRegistered<IThirdPartyNoticesService, ThirdPartyNoticesService>();
            serviceLocator.RegisterTypeIfNotYetRegistered<ICloseApplicationService, CloseApplicationService>();
            serviceLocator.RegisterTypeIfNotYetRegistered<IRecentlyUsedItemsService, RecentlyUsedItemsService>();
            serviceLocator.RegisterTypeIfNotYetRegistered<IKeyboardMappingsService, KeyboardMappingsService>();
            serviceLocator.RegisterTypeIfNotYetRegistered<IStatusFilterService, StatusFilterService>();
            serviceLocator.RegisterTypeIfNotYetRegistered<IStatusService, StatusService>();
            serviceLocator
                .RegisterTypeIfNotYetRegistered<ISplashScreenService,
                    SplashScreenService>();

            serviceLocator.RegisterTypeIfNotYetRegistered<ICommandInfoService, CommandInfoService>();
            serviceLocator.RegisterTypeIfNotYetRegistered<IAppDataService, AppDataService>();
            serviceLocator.RegisterTypeIfNotYetRegistered<IEnsureStartupService, EnsureStartupService>();
            serviceLocator.RegisterTypeIfNotYetRegistered<IAccentColorService, AccentColorService>();
            serviceLocator.RegisterTypeIfNotYetRegistered<IAboutInfoService, AboutInfoService>();
            serviceLocator.RegisterTypeIfNotYetRegistered<IAboutService, AboutService>();
            serviceLocator.RegisterTypeIfNotYetRegistered<IClipboardService, ClipboardService>();
            serviceLocator.RegisterTypeIfNotYetRegistered<IThemeService, ThemeService>();
            serviceLocator.RegisterTypeIfNotYetRegistered<IViewActivationService, ViewActivationService>();

            serviceLocator.RegisterType<IMessageService, MessageService>();

            // Hints system
            serviceLocator.RegisterType<IAdorneredTooltipsCollection, AdorneredTooltipsCollection>();
            serviceLocator.RegisterType<IAdornerLayer, HintsAdornerLayer>();
            serviceLocator.RegisterType<IAdorneredTooltipsManager, AdorneredTooltipsManager>();
            serviceLocator.RegisterType<IHintsProvider, HintsProvider>();

            serviceLocator.RegisterType<IAdornerLayer, HintsAdornerLayer>(RegistrationType.Transient);
        }

        public void Dispose()
        {
            _serviceLocator.ResolveType<PresetDataPersisterService>().CloseDatabase().Wait();
        }
    }
}