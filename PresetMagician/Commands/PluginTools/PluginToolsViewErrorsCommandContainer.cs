using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.Interfaces;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsViewErrorsCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly GlobalFrontendService _globalFrontendService;
        private Plugin _previouslySelectedPlugin;

        public PluginToolsViewErrorsCommandContainer(ICommandManager commandManager, IRuntimeConfigurationService runtimeConfigurationService
        )
            : base(Commands.PluginTools.ViewErrors, commandManager, runtimeConfigurationService)
        {
            _globalFrontendService = ServiceLocator.Default.ResolveType<GlobalFrontendService>();
            _uiVisualizerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();

            

            _globalFrontendService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
        }

        private void SelectedPluginOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            InvalidateCommand();
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _globalFrontendService.SelectedPlugins.Count > 0 && _globalFrontendService.SelectedPlugin != null;
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
            await _uiVisualizerService.ShowDialogAsync<VstPluginLogViewModel>(_globalFrontendService.SelectedPlugin);
        }
    }
}