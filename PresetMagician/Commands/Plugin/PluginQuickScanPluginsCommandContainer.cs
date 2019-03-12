using System.Collections.Generic;
using System.Linq;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.Interfaces;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginQuickScanPluginsCommandContainer : AbstractScanPluginsCommandContainer
    {
        public PluginQuickScanPluginsCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService, IVstService vstService,
            IApplicationService applicationService,
            IDispatcherService dispatcherService,
            IAdvancedMessageService messageService,
            INativeInstrumentsResourceGeneratorService resourceGeneratorService, PresetDataPersisterService presetDataPersisterService)
            : base(Commands.Plugin.QuickScanPlugins, commandManager, runtimeConfigurationService, vstService,
                applicationService, dispatcherService, messageService, resourceGeneratorService,presetDataPersisterService)
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