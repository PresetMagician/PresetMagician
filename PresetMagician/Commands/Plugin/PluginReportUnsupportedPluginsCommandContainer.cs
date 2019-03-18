using Catel.MVVM;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginReportUnsupportedPluginsCommandContainer : AbstractReportPluginsCommandContainer
    {
        public PluginReportUnsupportedPluginsCommandContainer(ICommandManager commandManager, IVstService vstService,
            ILicenseService licenseService, IApplicationService applicationService, GlobalService globalService,
            IRuntimeConfigurationService runtimeConfigurationService) : base(Commands.Plugin.ReportUnsupportedPlugins,
            commandManager, vstService, licenseService,
            applicationService, runtimeConfigurationService, globalService)
        {
            ReportAll = true;
        }
    }
}