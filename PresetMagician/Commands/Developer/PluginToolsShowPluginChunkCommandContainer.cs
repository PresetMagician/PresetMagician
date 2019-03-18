using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;
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
        private readonly GlobalFrontendService _globalFrontendService;

        public PluginToolsShowPluginChunkCommandContainer(ICommandManager commandManager,
            GlobalFrontendService globalFrontendService,
            RemoteVstService remoteVstService,
            IUIVisualizerService uiVisualizerService, IRuntimeConfigurationService runtimeConfigurationService,
            IViewModelFactory viewModelFactory)
            : base(Commands.PluginTools.ShowPluginChunk, commandManager, runtimeConfigurationService)
        {
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => viewModelFactory);

            _uiVisualizerService = uiVisualizerService;
            _viewModelFactory = viewModelFactory;
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
            var pluginInstance =
                await _remoteVstService.GetInteractivePluginInstance(_globalFrontendService.SelectedPlugin);

            var chunkViewModel = _viewModelFactory.CreateViewModel<VstPluginChunkViewModel>(pluginInstance);


            if (!pluginInstance.IsLoaded)
            {
                await pluginInstance.LoadPlugin();
            }


            var bankChunk = pluginInstance.GetChunk(false);
            if (!(bankChunk is null))
            {
                chunkViewModel.ChunkBankMemoryStream.SetLength(0);
                chunkViewModel.ChunkBankMemoryStream.Write(bankChunk, 0, bankChunk.Length);
            }

            var presetChunk = pluginInstance.GetChunk(false);
            if (!(presetChunk is null))
            {
                chunkViewModel.ChunkBankMemoryStream.SetLength(0);
                chunkViewModel.ChunkBankMemoryStream.Write(presetChunk, 0, presetChunk.Length);
            }

            await _uiVisualizerService.ShowDialogAsync(chunkViewModel);
        }
    }
}