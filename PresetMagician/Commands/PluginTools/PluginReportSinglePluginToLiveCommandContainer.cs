using System.Collections.Generic;
using System.Linq;
using Catel.Logging;
using Catel.MVVM;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using SharedModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsReportSinglePluginToLiveCommandContainer : AbstractReportPluginsCommandContainer
    {
        protected override ILog _log { get; set; } = LogManager.GetCurrentClassLogger();

        public PluginToolsReportSinglePluginToLiveCommandContainer(ICommandManager commandManager,
            IVstService vstService,
            ILicenseService licenseService, IApplicationService applicationService,
            IRuntimeConfigurationService runtimeConfigurationService) : base(
            Commands.PluginTools.ReportSinglePluginToLive, commandManager, vstService, licenseService,
            applicationService, runtimeConfigurationService)
        {
        }

        protected override bool CanExecute(object parameter)
        {
            return true;
        }

        protected override List<Plugin> GetPluginsToReport()
        {
            return (from plugin in _vstService.SelectedPlugins
                where plugin.IsScanned
                select plugin).ToList();
        }

        protected override string GetPluginReportSite()
        {
            return Settings.Links.SubmitPluginsLive;
        }
    }
}