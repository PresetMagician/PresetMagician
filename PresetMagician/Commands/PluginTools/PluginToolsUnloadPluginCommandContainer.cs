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
    public class PluginToolsUnloadPluginCommandContainer : CommandContainerBase
    {
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IVstService _vstService;

        public PluginToolsUnloadPluginCommandContainer(ICommandManager commandManager, IVstService vstService,
            IUIVisualizerService uiVisualizerService)
            : base(Commands.PluginTools.UnloadPlugin, commandManager)
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
            if (_vstService.SelectedPlugin.IsLoaded)
            {
                _vstService.VstHost.UnloadVST(_vstService.SelectedPlugin);
            }
        }
    }
}