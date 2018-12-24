using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using PresetMagicianShell.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsDisablePluginsCommandContainer : CommandContainerBase
    {
        private readonly IVstService _vstService;

        public PluginToolsDisablePluginsCommandContainer(ICommandManager commandManager, IVstService vstService)
            : base(Commands.PluginTools.DisablePlugins, commandManager)
        {
            Argument.IsNotNull(() => vstService);

            _vstService = vstService;

            _vstService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return _vstService.SelectedPlugins.Count > 0;
        }

        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            foreach (var plugin in _vstService.SelectedPlugins)
            {
                plugin.Enabled = false;
            }
        }
    }
}