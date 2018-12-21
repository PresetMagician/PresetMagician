using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using System.Collections.Specialized;
using Catel.Data;
using Newtonsoft.Json.Linq;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services;
using PresetMagicianShell.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
{
    // ReSharper disable once UnusedMember.Global
    public class PluginReportUnsupportedPluginsCommandContainer : AbstractReportPluginsCommandContainer
    {
        protected override ILog _log { get; set; } = LogManager.GetCurrentClassLogger();

        public PluginReportUnsupportedPluginsCommandContainer(ICommandManager commandManager, IVstService vstService,
            ILicenseService licenseService, IApplicationService applicationService,
            IRuntimeConfigurationService runtimeConfigurationService) : base(Commands.Plugin.ReportUnsupportedPlugins, commandManager, vstService, licenseService,
            applicationService, runtimeConfigurationService)
        {
            ReportAll = true;
        }
    }
}