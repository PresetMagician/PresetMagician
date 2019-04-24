using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.Services;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsShowPluginChunkCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly RemoteVstService _remoteVstService;

        public PluginToolsShowPluginChunkCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator)
            : base(Commands.PluginTools.ShowPluginChunk, commandManager, serviceLocator)
        {
            _uiVisualizerService = ServiceLocator.ResolveType<IUIVisualizerService>();
            _viewModelFactory = ServiceLocator.ResolveType<IViewModelFactory>();
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

            var chunkViewModel = _viewModelFactory.CreateViewModel<VstPluginChunkViewModel>(pluginInstance);


            if (!pluginInstance.IsLoaded)
            {
                await pluginInstance.LoadPlugin();
            }

            chunkViewModel.RefreshChunks();

            await _uiVisualizerService.ShowDialogAsync(chunkViewModel);
        }
    }
}