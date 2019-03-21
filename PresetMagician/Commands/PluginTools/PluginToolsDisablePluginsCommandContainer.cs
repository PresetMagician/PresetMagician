using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Services;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsDisablePluginsCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly GlobalFrontendService _globalFrontendService;
        private readonly DataPersisterService _dataPersisterService;

        public PluginToolsDisablePluginsCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator)
            : base(Commands.PluginTools.DisablePlugins, commandManager, serviceLocator)
        {
            _dataPersisterService = ServiceLocator.ResolveType<DataPersisterService>();
            _globalFrontendService = ServiceLocator.ResolveType<GlobalFrontendService>();
            _globalFrontendService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _globalFrontendService.SelectedPlugins.Count > 0;
        }

        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            foreach (var plugin in _globalFrontendService.SelectedPlugins)
            {
                plugin.IsEnabled = false;
            }

            _dataPersisterService.SavePlugins();
        }
    }
}