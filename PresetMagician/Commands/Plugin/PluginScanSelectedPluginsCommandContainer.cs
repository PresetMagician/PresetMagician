using System.Collections.Generic;
using System.Linq;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using SharedModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginScanSelectedPluginsCommandContainer : AbstractScanPluginsCommandContainer
    {
        public PluginScanSelectedPluginsCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService, IVstService vstService,
            IApplicationService applicationService,
            IDispatcherService dispatcherService, IDatabaseService databaseService,
            INativeInstrumentsResourceGeneratorService resourceGeneratorService)
            : base(Commands.Plugin.ScanSelectedPlugins, commandManager, runtimeConfigurationService, vstService,
                applicationService, dispatcherService, databaseService, resourceGeneratorService)
        {
            vstService.SelectedPlugins.CollectionChanged += OnPluginsListChanged;
        }

        protected override List<Plugin> GetPluginsToScan()
        {
            return (from plugin in _vstService.SelectedPlugins where plugin.IsEnabled select plugin).ToList();
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) &&
                   _vstService.SelectedPlugins.Count > 0;
        }
    }
}