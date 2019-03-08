using System.Collections.Generic;
using System.Linq;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using SharedModels;
using SharedModels.Models;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginQuickScanPluginsCommandContainer : AbstractScanPluginsCommandContainer
    {
        public PluginQuickScanPluginsCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService, IVstService vstService,
            IApplicationService applicationService,
            IDispatcherService dispatcherService, IDatabaseService databaseService,
            IAdvancedMessageService messageService,
            INativeInstrumentsResourceGeneratorService resourceGeneratorService)
            : base(Commands.Plugin.QuickScanPlugins, commandManager, runtimeConfigurationService, vstService,
                applicationService, dispatcherService, messageService, databaseService, resourceGeneratorService)
        {
        }


        protected override bool IsQuickAnalysisMode()
        {
            return true;
        }

        protected override List<Plugin> GetPluginsToScan()
        {
            return (from plugin in _vstService.Plugins where plugin.IsEnabled select plugin).ToList();
        }
    }
}