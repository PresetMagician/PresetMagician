using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsViewSettingsCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly GlobalFrontendService _globalFrontendService;
        private Plugin _previouslySelectedPlugin;

        public PluginToolsViewSettingsCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService
        )
            : base(Commands.PluginTools.ViewSettings, commandManager, runtimeConfigurationService)
        {
            _uiVisualizerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();
            _globalFrontendService = ServiceLocator.Default.ResolveType<GlobalFrontendService>();

            _globalFrontendService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
        }


        private void SelectedPluginOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            InvalidateCommand();
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _globalFrontendService.SelectedPlugins.Count > 0 &&
                   _globalFrontendService.SelectedPlugin != null &&
                   _globalFrontendService.SelectedPlugin.HasMetadata;
        }

        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            if (_previouslySelectedPlugin != null)
            {
                _previouslySelectedPlugin.PropertyChanged -= SelectedPluginOnPropertyChanged;
            }

            if (_globalFrontendService.SelectedPlugin != null)
            {
                _globalFrontendService.SelectedPlugin.PropertyChanged += SelectedPluginOnPropertyChanged;
                _previouslySelectedPlugin = _globalFrontendService.SelectedPlugin;
            }

            InvalidateCommand();
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            await _uiVisualizerService.ShowDialogAsync<VstPluginSettingsViewModel>(
                _globalFrontendService.SelectedPlugin);
        }
    }
}