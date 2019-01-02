using System.Collections.Specialized;
using Catel;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Services.Interfaces;
using Xceed.Wpf.AvalonDock.Layout;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsViewSettingsCommandContainer : CommandContainerBase
    {
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private readonly IVstService _vstService;

        public PluginToolsViewSettingsCommandContainer(ICommandManager commandManager, IVstService vstService,
            IRuntimeConfigurationService runtimeConfigurationService
        )
            : base(Commands.PluginTools.ViewSettings, commandManager)
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
            var pluginSettings = ServiceLocator.Default.ResolveType<LayoutAnchorable>("PluginSettings");
            pluginSettings.ToggleAutoHide();
            
            
        }
    }
}