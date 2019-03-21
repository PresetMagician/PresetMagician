using Catel.IoC;
using Catel.Services;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Orchestra.Services;
using PresetMagician.Core;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Services;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using AboutInfoService = Orchestra.Services.AboutInfoService;

namespace PresetMagician
{
    public static class FrontendInitializer
    {
        public static void RegisterTypes(IServiceLocator serviceLocator)
        {
            CoreInitializer.RegisterServices(serviceLocator);

            serviceLocator.RegisterType<IAboutInfoService, AboutInfoService>();
            serviceLocator.RegisterType<ICustomStatusService, CustomStatusService>();
            serviceLocator.RegisterType<IPleaseWaitService, CustomPleaseWaitService>();
            serviceLocator.RegisterType<ILicenseService, LicenseService>();
            serviceLocator.RegisterType<IRuntimeConfigurationService, RuntimeConfigurationService>();
            serviceLocator.RegisterType<IVstService, VstService>();
            serviceLocator.RegisterType<IApplicationService, ApplicationService>();
            serviceLocator.RegisterType<IAdvancedMessageService, AdvancedMessageService>();
            serviceLocator.RegisterType<FrontendService, FrontendService>();

            serviceLocator
                .RegisterType<INativeInstrumentsResourceGeneratorService, NativeInstrumentsResourceGeneratorService>();
        }

        public static void Initialize(IServiceLocator serviceLocator)
        {
            VendorPresetParserInitializer.Initialize(serviceLocator.ResolveType<VendorPresetParserService>());
            serviceLocator.ResolveType<IApplicationService>().Initialize();
        }
    }
}