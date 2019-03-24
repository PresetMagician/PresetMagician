using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Services;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsShowPluginEditorCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly RemoteVstService _remoteVstService;

        public PluginToolsShowPluginEditorCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator)
            : base(Commands.PluginTools.ShowPluginEditor, commandManager, serviceLocator)
        {
            _remoteVstService = ServiceLocator.ResolveType<RemoteVstService>();

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
                _remoteVstService.GetInteractivePluginInstance(_globalFrontendService.SelectedPlugin);

            if (!pluginInstance.IsLoaded)
            {
                await pluginInstance.LoadPlugin();
            }

            pluginInstance.OpenEditor(false);
        }
    }
}