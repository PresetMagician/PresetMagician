using System.Collections.Generic;
using System.Linq;
using Catel.MVVM;
using PresetMagician.Core.Interfaces;
using PresetMagician.Services.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginForceReportPluginsToLiveCommandContainer : AbstractReportPluginsCommandContainer
    {
        public PluginForceReportPluginsToLiveCommandContainer(ICommandManager commandManager,
            IVstService vstService,
            ILicenseService licenseService, IApplicationService applicationService,
            IRuntimeConfigurationService runtimeConfigurationService, GlobalService globalService) : base(
            Commands.Plugin.ForceReportPluginsToLive, commandManager, vstService, licenseService,
            applicationService, runtimeConfigurationService, globalService)
        {
            ReportAll = true;
        }

        protected override bool CanExecute(object parameter)
        {
            return true;
        }

        protected override List<Plugin> GetPluginsToReport()
        {
            return (from plugin in _vstService.Plugins
                where plugin.HasMetadata
                select plugin).ToList();
        }
        
        protected override string GetPluginReportSite()
        {
            return Settings.Links.SubmitPluginsLive;
        }

    }
}