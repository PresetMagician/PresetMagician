using System.Collections.Generic;
using System.Linq;
using Catel.MVVM;
using PresetMagician.Core.Models;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsReportSinglePluginToLiveCommandContainer : AbstractReportPluginsCommandContainer
    {
        public PluginToolsReportSinglePluginToLiveCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService) : base(
            Commands.PluginTools.ReportSinglePluginToLive, commandManager, runtimeConfigurationService)
        {
        }

        protected override bool CanExecute(object parameter)
        {
            return true;
        }

        protected override List<Plugin> GetPluginsToReport()
        {
            return (from plugin in GlobalFrontendService.SelectedPlugins
                where plugin.HasMetadata
                select plugin).ToList();
        }

        protected override string GetPluginReportSite()
        {
            return Settings.Links.SubmitPluginsLive;
        }
    }
}