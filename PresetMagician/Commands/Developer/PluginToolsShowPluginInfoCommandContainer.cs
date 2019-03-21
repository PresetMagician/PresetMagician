using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsShowPluginInfoCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IUIVisualizerService _uiVisualizerService;

        public PluginToolsShowPluginInfoCommandContainer(ICommandManager commandManager, IServiceLocator serviceLocator)
            : base(Commands.PluginTools.ShowPluginInfo, commandManager, serviceLocator)
        {
            _uiVisualizerService = ServiceLocator.ResolveType<IUIVisualizerService>();

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