using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;
using Xceed.Wpf.AvalonDock.Layout;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsViewSettingsCommandContainer : CommandContainerBase
    {
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IVstService _vstService;
        private Plugin previouslySelectedPlugin;

        public PluginToolsViewSettingsCommandContainer(ICommandManager commandManager, IVstService vstService,
            IUIVisualizerService uiVisualizerService
        )
            : base(Commands.PluginTools.ViewSettings, commandManager)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => uiVisualizerService);

            _vstService = vstService;
            _vstService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;

            _uiVisualizerService = uiVisualizerService;
        }

        private void SelectedPluginOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            InvalidateCommand();
        }

        protected override bool CanExecute(object parameter)
        {
            return _vstService.SelectedPlugins.Count > 0 && _vstService.SelectedPlugin.IsScanned;
        }

        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            if (previouslySelectedPlugin != null)
            {
                previouslySelectedPlugin.PropertyChanged -= SelectedPluginOnPropertyChanged;
            }

            if (_vstService.SelectedPlugin != null)
            {
                _vstService.SelectedPlugin.PropertyChanged += SelectedPluginOnPropertyChanged;
                previouslySelectedPlugin = _vstService.SelectedPlugin;
            }

            InvalidateCommand();
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            await _uiVisualizerService.ShowDialogAsync<VstPluginViewModel>(_vstService.SelectedPlugin);
        }
    }
}