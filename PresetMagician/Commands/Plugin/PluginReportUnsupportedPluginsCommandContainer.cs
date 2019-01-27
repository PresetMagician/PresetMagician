using Catel.Logging;
using Catel.MVVM;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginReportUnsupportedPluginsCommandContainer : AbstractReportPluginsCommandContainer
    {
        protected override ILog _log { get; set; } = LogManager.GetCurrentClassLogger();

        public PluginReportUnsupportedPluginsCommandContainer(ICommandManager commandManager, IVstService vstService,
            ILicenseService licenseService, IApplicationService applicationService,
            IRuntimeConfigurationService runtimeConfigurationService) : base(Commands.Plugin.ReportUnsupportedPlugins,
            commandManager, vstService, licenseService,
            applicationService, runtimeConfigurationService)
        {
            ReportAll = true;
        }
    }
}