using Catel.IoC;
using Catel.MVVM;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginReportUnsupportedPluginsCommandContainer : AbstractReportPluginsCommandContainer
    {
        public PluginReportUnsupportedPluginsCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator) : base(Commands.Plugin.ReportUnsupportedPlugins,
            commandManager, serviceLocator)
        {
            ReportAll = true;
        }
    }
}