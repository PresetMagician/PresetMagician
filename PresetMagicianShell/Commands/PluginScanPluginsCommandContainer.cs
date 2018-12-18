using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Catel;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using PresetMagicianShell.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
{
    public class PluginScanPluginsCommandContainer : CommandContainerBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IRuntimeConfigurationService _runtimeConfigurationService;

        public PluginScanPluginsCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.Plugin.ScanPlugins, commandManager)
        {
            Argument.IsNotNull(() => runtimeConfigurationService);

            _runtimeConfigurationService = runtimeConfigurationService;
            _runtimeConfigurationService.RuntimeConfiguration.Plugins.CollectionChanged += OnPluginsListChanged;

        }

        protected override bool CanExecute(object parameter)
        {
            return (_runtimeConfigurationService.RuntimeConfiguration.Plugins.Count > 0);
        }

        void OnPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            base.Execute(parameter);
        }
    }
}