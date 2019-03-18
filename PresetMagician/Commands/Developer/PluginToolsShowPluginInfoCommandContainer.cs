using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsShowPluginInfoCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly GlobalFrontendService _globalFrontendService;
        private readonly IUIVisualizerService _uiVisualizerService;

        public PluginToolsShowPluginInfoCommandContainer(ICommandManager commandManager,GlobalFrontendService globalFrontendService,
            IUIVisualizerService uiVisualizerService, IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.PluginTools.ShowPluginInfo, commandManager, runtimeConfigurationService)
        {
            _globalFrontendService = globalFrontendService;
            _uiVisualizerService = uiVisualizerService;

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
            await _uiVisualizerService.ShowDialogAsync<VstPluginInfoViewModel>(_globalFrontendService.SelectedPlugin);
        }
    }
}