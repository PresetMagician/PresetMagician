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
    public class PluginScanPluginsCommandContainer : AbstractScanPluginsCommandContainer
    {
        public PluginScanPluginsCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.Plugin.ScanPlugins, commandManager, runtimeConfigurationService)
        {
        }

        protected override List<Plugin> GetPluginsToScan()
        {
            return (from plugin in GlobalService.Plugins where plugin.IsEnabled select plugin).ToList();
        }
    }
}