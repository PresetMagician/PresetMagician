using System.Collections.Generic;
using System.Linq;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Models;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginScanPluginsCommandContainer : AbstractScanPluginsCommandContainer
    {
        public PluginScanPluginsCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator)
            : base(Commands.Plugin.ScanPlugins, commandManager, serviceLocator)
        {
        }

        protected override List<Plugin> GetPluginsToScan()
        {
            return (from plugin in GlobalService.Plugins where plugin.IsEnabled select plugin).ToList();
        }
    }
}