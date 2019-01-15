using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsEnablePluginsCommandContainer : CommandContainerBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly IVstService _vstService;

        public PluginToolsEnablePluginsCommandContainer(ICommandManager commandManager, IVstService vstService,
            IDatabaseService databaseService
        )
            : base(Commands.PluginTools.EnablePlugins, commandManager)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => databaseService);

            _vstService = vstService;
            _vstService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
            _databaseService = databaseService;
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
                plugin.IsEnabled = true;
            }

            await _vstService.SavePlugins();
        }
    }
}