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
    public class PluginToolsShowPluginChunkCommandContainer : CommandContainerBase
    {
        private readonly IVstService _vstService;
        private readonly IUIVisualizerService _uiVisualizerService;

        public PluginToolsShowPluginChunkCommandContainer(ICommandManager commandManager, IVstService vstService,
            IUIVisualizerService uiVisualizerService)
            : base(Commands.PluginTools.ShowPluginChunk, commandManager)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => uiVisualizerService);

            _vstService = vstService;
            _uiVisualizerService = uiVisualizerService;

            _vstService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return _vstService.SelectedPlugins.Count == 1;
        }

        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            var pluginInstance = _vstService.GetInteractivePluginInstance(_vstService.SelectedPlugin);

            if (!pluginInstance.IsLoaded)
            {
                await pluginInstance.LoadPlugin();
            }


            var bankChunk = pluginInstance.GetChunk(false);
            if (!(bankChunk is null))
            {
                pluginInstance.Plugin.ChunkBankMemoryStream.SetLength(0);
                pluginInstance.Plugin.ChunkBankMemoryStream.Write(bankChunk, 0, bankChunk.Length);
            }

            var presetChunk = pluginInstance.GetChunk(false);
            if (!(presetChunk is null))
            {
                pluginInstance.Plugin.ChunkBankMemoryStream.SetLength(0);
                pluginInstance.Plugin.ChunkBankMemoryStream.Write(presetChunk, 0, presetChunk.Length);
            }

            await _uiVisualizerService.ShowDialogAsync<VstPluginChunkViewModel>(_vstService.SelectedPlugin);
        }
    }
}