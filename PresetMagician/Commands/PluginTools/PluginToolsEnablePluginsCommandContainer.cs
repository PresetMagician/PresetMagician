using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsEnablePluginsCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly GlobalFrontendService _globalFrontendService;
        private readonly DataPersisterService _dataPersisterService;

        public PluginToolsEnablePluginsCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService
        )
            : base(Commands.PluginTools.EnablePlugins, commandManager, runtimeConfigurationService)
        {
            _globalFrontendService = ServiceLocator.Default.ResolveType<GlobalFrontendService>();
            _dataPersisterService = ServiceLocator.Default.ResolveType<DataPersisterService>();
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
                plugin.IsEnabled = true;
            }

            _dataPersisterService.SavePlugins();
        }
    }
}