using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel.MVVM;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsUnloadPluginCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly RemoteVstService _remoteVstService;
        private readonly GlobalFrontendService _globalFrontendService;

        public PluginToolsUnloadPluginCommandContainer(ICommandManager commandManager,
            GlobalFrontendService globalFrontendService,
            RemoteVstService remoteVstService,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.PluginTools.UnloadPlugin, commandManager, runtimeConfigurationService)
        {
            _remoteVstService = remoteVstService;
            _globalFrontendService = globalFrontendService;
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
            var pluginInstance =
                await _remoteVstService.GetInteractivePluginInstance(_globalFrontendService.SelectedPlugin);

            if (pluginInstance.IsLoaded)
            {
                pluginInstance.UnloadPlugin();
            }
        }
    }
}