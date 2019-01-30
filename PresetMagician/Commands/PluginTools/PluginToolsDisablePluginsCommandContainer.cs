using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsDisablePluginsCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IVstService _vstService;

        public PluginToolsDisablePluginsCommandContainer(ICommandManager commandManager, IVstService vstService,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.PluginTools.DisablePlugins, commandManager, runtimeConfigurationService)
        {
            Argument.IsNotNull(() => vstService);

            _vstService = vstService;

            _vstService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _vstService.SelectedPlugins.Count > 0;
        }
        
        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            foreach (var plugin in _vstService.SelectedPlugins)
            {
                plugin.IsEnabled = false;
            }

            await _vstService.SavePlugins();
        }
    }
}