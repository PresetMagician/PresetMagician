using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsShowPluginChunkCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IVstService _vstService;
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IViewModelFactory _viewModelFactory;

        public PluginToolsShowPluginChunkCommandContainer(ICommandManager commandManager, IVstService vstService,
            IUIVisualizerService uiVisualizerService, IRuntimeConfigurationService runtimeConfigurationService, IViewModelFactory viewModelFactory)
            : base(Commands.PluginTools.ShowPluginChunk, commandManager, runtimeConfigurationService)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => viewModelFactory);

            _vstService = vstService;
            _uiVisualizerService = uiVisualizerService;
            _viewModelFactory = viewModelFactory;

            _vstService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _vstService.SelectedPlugins.Count == 1;
        }

        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            var pluginInstance = await _vstService.GetInteractivePluginInstance(_vstService.SelectedPlugin);
            
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