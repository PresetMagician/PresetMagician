using System.Collections.Specialized;
using Catel;
using Catel.MVVM;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsDisablePluginsCommandContainer : CommandContainerBase
    {
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private readonly IVstService _vstService;

        public PluginToolsDisablePluginsCommandContainer(ICommandManager commandManager, IVstService vstService,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.PluginTools.DisablePlugins, commandManager)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => runtimeConfigurationService);

            _vstService = vstService;

            _vstService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
            _runtimeConfigurationService = runtimeConfigurationService;
        }

        protected override bool CanExecute(object parameter)
        {
            return _vstService.SelectedPlugins.Count > 0;
        }

        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }


        protected override void Execute(object parameter)
        {
            foreach (var plugin in _vstService.SelectedPlugins)
            {
                plugin.Enabled = false;
            }

            _runtimeConfigurationService.SaveConfiguration();
        }
    }
}