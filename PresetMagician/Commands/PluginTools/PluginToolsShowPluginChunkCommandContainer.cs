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

        public PluginToolsShowPluginChunkCommandContainer(ICommandManager commandManager, IVstService vstService, IUIVisualizerService uiVisualizerService)
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
            var plugin = _vstService.SelectedPlugin;
            
            if (!_vstService.SelectedPlugin.IsLoaded)
            {
                await _vstService.LoadVstInteractive(_vstService.SelectedPlugin);
            }

            var bankChunk = _vstService.SelectedPlugin.RemoteVstService.GetChunk(_vstService.SelectedPlugin.Guid, false);
            if (!(bankChunk is null))
            {
                plugin.ChunkBankMemoryStream.SetLength(0);
                plugin.ChunkBankMemoryStream.Write(bankChunk, 0, bankChunk.Length);

            }
            
            var presetChunk = _vstService.SelectedPlugin.RemoteVstService.GetChunk(_vstService.SelectedPlugin.Guid, true);
            if (!(presetChunk is null))
            {
                plugin.ChunkBankMemoryStream.SetLength(0);
                plugin.ChunkBankMemoryStream.Write(presetChunk, 0, presetChunk.Length);
            }
           
            await _uiVisualizerService.ShowDialogAsync<VstPluginChunkViewModel>(_vstService.SelectedPlugin);
        }
    }
}