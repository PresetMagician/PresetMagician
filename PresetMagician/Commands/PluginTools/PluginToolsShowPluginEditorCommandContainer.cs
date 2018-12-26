using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;
using PresetMagician.Views;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsShowPluginEditorCommandContainer : CommandContainerBase
    {
        private readonly IVstService _vstService;
        private readonly IUIVisualizerService _uiVisualizerService;

        public PluginToolsShowPluginEditorCommandContainer(ICommandManager commandManager, IVstService vstService, IUIVisualizerService uiVisualizerService)
            : base(Commands.PluginTools.ShowPluginEditor, commandManager)
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
            if (!_vstService.SelectedPlugin.IsLoaded)
            {
                _vstService.VstHost.LoadVST(_vstService.SelectedPlugin);
            }
            PluginEditorFrame dlg = new PluginEditorFrame();
            dlg.PluginCommandStub = _vstService.SelectedPlugin.PluginContext.PluginCommandStub;

            dlg.Show();
            //dlg.ShowDialog();
        }
    }
}