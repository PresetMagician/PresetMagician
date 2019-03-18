using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsLoadPluginCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly GlobalFrontendService _globalFrontendService;
        private readonly RemoteVstService _remoteVstService;

        public PluginToolsLoadPluginCommandContainer(ICommandManager commandManager, 
            IRuntimeConfigurationService runtimeConfigurationService, GlobalFrontendService globalFrontendService, RemoteVstService remoteVstService)
            : base(Commands.PluginTools.LoadPlugin, commandManager, runtimeConfigurationService)
        {
            _globalFrontendService = globalFrontendService;

            _remoteVstService = remoteVstService;

            _globalFrontendService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _globalFrontendService.SelectedPlugins.Count == 1;
        }

        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            var pluginInstance = await _remoteVstService.GetInteractivePluginInstance(_globalFrontendService.SelectedPlugin);

            if (!pluginInstance.IsLoaded)
            {
                await pluginInstance.LoadPlugin();
            }
        }
    }
}