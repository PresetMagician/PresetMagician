using Catel.IoC;
using PresetMagician.Core.Services;

namespace PresetMagician.Core
{
    public class Core
    {
        public static void RegisterServices()
        {
            var serviceLocator = ServiceLocator.Default;
            serviceLocator.RegisterType<GlobalService, GlobalService>();
            serviceLocator.RegisterType<DataPersisterService, DataPersisterService>();
            serviceLocator.RegisterType<PresetDataPersisterService, PresetDataPersisterService>();
            serviceLocator.RegisterType<PluginService, PluginService>();
            serviceLocator.RegisterType<CharacteristicsService, CharacteristicsService>();
            serviceLocator.RegisterType<TypesService, TypesService>();
        }
    }
}