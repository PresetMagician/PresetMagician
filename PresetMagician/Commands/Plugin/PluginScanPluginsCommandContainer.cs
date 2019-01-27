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
    public class PluginScanPluginsCommandContainer : AbstractScanPluginsCommandContainer
    {
        public PluginScanPluginsCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService, IVstService vstService,
            IApplicationService applicationService,
            IDispatcherService dispatcherService, IDatabaseService databaseService,
            INativeInstrumentsResourceGeneratorService resourceGeneratorService)
            : base(Commands.Plugin.ScanPlugins, commandManager, runtimeConfigurationService, vstService,
                applicationService, dispatcherService, databaseService, resourceGeneratorService)
        {
        }

        protected override List<Plugin> GetPluginsToScan()
        {
            return (from plugin in _vstService.Plugins where plugin.IsEnabled select plugin).ToList();
        }
    }
}