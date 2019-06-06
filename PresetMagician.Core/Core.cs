using System;
using System.Reflection;
using Catel.IoC;
using PresetMagician.Core.Services;
using PresetMagician.Utils;

namespace PresetMagician.Core
{
    public static class CoreInitializer
    {
        public static void RegisterServices(IServiceLocator serviceLocator)
        {
            serviceLocator.RegisterType<GlobalService, GlobalService>();
            serviceLocator.RegisterType<GlobalFrontendService, GlobalFrontendService>();
            serviceLocator.RegisterType<DataPersisterService, DataPersisterService>();
            serviceLocator.RegisterType<PresetDataPersisterService, PresetDataPersisterService>();
            serviceLocator.RegisterType<PluginService, PluginService>();
            serviceLocator.RegisterType<CharacteristicsService, CharacteristicsService>();
            serviceLocator.RegisterType<TypesService, TypesService>();
            serviceLocator.RegisterType<VendorPresetParserService, VendorPresetParserService>();
            serviceLocator.RegisterType<RemoteVstService, RemoteVstService>();
            serviceLocator.RegisterType<DeveloperService, DeveloperService>();
            serviceLocator.RegisterType<AudioService, AudioService>();
            serviceLocator.RegisterType<MidiService, MidiService>();
        }
    }

    public static class Core
    {
        public static bool UseDispatcher = true;
    }

    public static class MethodTimeLogger
    {
        public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)
        {
            GlobalMethodTimeLogger.Log(methodBase, elapsed);
        }
    }
}