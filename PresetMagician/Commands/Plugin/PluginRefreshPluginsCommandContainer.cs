using System;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Catel;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Commands.Plugin;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginRefreshPluginsCommandContainer : ThreadedApplicationNotBusyCommandContainer
    {
        private readonly IApplicationService _applicationService;
        private readonly PluginService _pluginService;
        private readonly GlobalService _globalService;
        private readonly DataPersisterService _dataPersisterService;

        public PluginRefreshPluginsCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator)
            : base(Commands.Plugin.RefreshPlugins, commandManager, serviceLocator)
        {
            _pluginService = ServiceLocator.ResolveType<PluginService>();
            _applicationService = ServiceLocator.ResolveType<IApplicationService>();
            _globalService = ServiceLocator.ResolveType<GlobalService>();
            _dataPersisterService = ServiceLocator.ResolveType<DataPersisterService>();
        }


        protected override async Task ExecuteThreaded(object parameter)
        {
            await ServiceLocator.ResolveType<RefreshPluginsCommand>().ExecuteAsync();
        }
    }
}