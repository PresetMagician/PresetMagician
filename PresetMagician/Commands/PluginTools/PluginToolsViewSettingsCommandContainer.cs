using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;
using SharedModels;
using SharedModels.Models;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsViewSettingsCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IVstService _vstService;
        private Plugin _previouslySelectedPlugin;

        public PluginToolsViewSettingsCommandContainer(ICommandManager commandManager, IVstService vstService,
            IUIVisualizerService uiVisualizerService,IRuntimeConfigurationService runtimeConfigurationService
        )
            : base(Commands.PluginTools.ViewSettings, commandManager, runtimeConfigurationService)
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
            return base.CanExecute(parameter) && _vstService.SelectedPlugins.Count > 0 &&
                   _vstService.SelectedPlugin != null &&
                   _vstService.SelectedPlugin.HasMetadata;
        }

        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            if (_previouslySelectedPlugin != null)
            {
                _previouslySelectedPlugin.PropertyChanged -= SelectedPluginOnPropertyChanged;
            }

            if (_vstService.SelectedPlugin != null)
            {
                _vstService.SelectedPlugin.PropertyChanged += SelectedPluginOnPropertyChanged;
                _previouslySelectedPlugin = _vstService.SelectedPlugin;
            }

            InvalidateCommand();
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            await _uiVisualizerService.ShowDialogAsync<VstPluginSettingsViewModel>(_vstService.SelectedPlugin);
        }
    }
}