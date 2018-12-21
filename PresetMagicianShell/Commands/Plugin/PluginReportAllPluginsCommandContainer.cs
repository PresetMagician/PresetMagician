using Catel.Logging;
using Catel.MVVM;
using PresetMagicianShell.Services;
using PresetMagicianShell.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
{
    // ReSharper disable once UnusedMember.Global
    public class PluginReportAllPluginsCommandContainer : AbstractReportPluginsCommandContainer
    {
        protected override ILog _log { get; set; } = LogManager.GetCurrentClassLogger();

        public PluginReportAllPluginsCommandContainer(ICommandManager commandManager, IVstService vstService,
            ILicenseService licenseService, IApplicationService applicationService,
            IRuntimeConfigurationService runtimeConfigurationService) : base(Commands.Plugin.ReportAllPlugins, commandManager, vstService, licenseService,
            applicationService, runtimeConfigurationService)
        {
            ReportAll = true;
        }
    }
}