using Catel.MVVM;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginReportUnsupportedPluginsCommandContainer : AbstractReportPluginsCommandContainer
    {
        public PluginReportUnsupportedPluginsCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService) : base(Commands.Plugin.ReportUnsupportedPlugins,
            commandManager, runtimeConfigurationService)
        {
            ReportAll = true;
        }
    }
}