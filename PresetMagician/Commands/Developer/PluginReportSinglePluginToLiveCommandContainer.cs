using System.Collections.Generic;
using System.Linq;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Models;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsReportSinglePluginToLiveCommandContainer : AbstractReportPluginsCommandContainer
    {
        public PluginToolsReportSinglePluginToLiveCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator) : base(
            Commands.PluginTools.ReportSinglePluginToLive, commandManager, serviceLocator)
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