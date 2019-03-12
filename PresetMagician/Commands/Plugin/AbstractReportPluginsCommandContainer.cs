using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Anotar.Catel;
using Catel;
using Catel.Data;
using Catel.MVVM;
using Newtonsoft.Json.Linq;
using Orchestra;
using PresetMagician.Services.Interfaces;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public abstract class AbstractReportPluginsCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IApplicationService _applicationService;
        private readonly ILicenseService _licenseService;
        protected readonly IVstService _vstService;
        protected bool ReportAll;

        protected AbstractReportPluginsCommandContainer(string command, ICommandManager commandManager,
            IVstService vstService,
            ILicenseService licenseService, IApplicationService applicationService,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(command, commandManager, runtimeConfigurationService)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => licenseService);
            Argument.IsNotNull(() => applicationService);

            _vstService = vstService;
            _licenseService = licenseService;
            _applicationService = applicationService;

            var wrapper = new ChangeNotificationWrapper(_vstService.Plugins);
            wrapper.CollectionItemPropertyChanged += OnPluginItemPropertyChanged;
            wrapper.CollectionChanged += OnPluginListChanged;

            ReportAll = false;
        }

        protected override bool CanExecute(object parameter)
        {
            int numPluginsToReport;

            if (ReportAll)
            {
                numPluginsToReport = (from plugin in _vstService.Plugins
                    where plugin.IsAnalyzed && !plugin.DontReport && !plugin.IsReported
                    select plugin).Count();
            }
            else
            {
                numPluginsToReport = (from plugin in _vstService.Plugins
                    where plugin.IsAnalyzed && plugin.IsSupported == false && !plugin.DontReport && !plugin.IsReported
                    select plugin).Count();
            }

            return base.CanExecute(parameter) && numPluginsToReport > 0;
        }

        private void OnPluginListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }

        private void OnPluginItemPropertyChanged(object o, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName == nameof(Plugin.IsSupported))
            {
                InvalidateCommand();
            }
        }

        protected virtual List<Plugin> GetPluginsToReport()
        {
            if (ReportAll)
            {
                return (from plugin in _vstService.Plugins
                    where plugin.IsAnalyzed && plugin.HasMetadata && !plugin.DontReport && !plugin.IsReported
                    select plugin).ToList();
            }


            return (from plugin in _vstService.Plugins
                where plugin.IsAnalyzed && plugin.HasMetadata && plugin.IsSupported == false && !plugin.DontReport && !plugin.IsReported
                select plugin).ToList();
        }

        protected virtual string GetPluginReportSite()
        {
            return Settings.Links.SubmitPlugins;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var pluginsToReport = GetPluginsToReport();

            var pluginReport = JObject.FromObject(new
            {
                pluginSubmission = new
                {
                    email = _licenseService.GetCurrentLicense().Customer.Email,
                    plugins =
                        from p in pluginsToReport
                        orderby p.VstPluginId
                        select new
                        {
                            vendorName = p.PluginVendor,
                            pluginName = p.PluginName,
                            pluginId = p.VstPluginId,
                            pluginSupported = p.IsSupported,
                            pluginPresetParser = p.PresetParser != null ? p.PresetParser.PresetParserType : "",
                            pluginSupportedSince = VersionHelper.GetCurrentVersion(),
                            pluginType = p.PluginTypeDescription,
                            pluginCapabilities = p.PluginCapabilities,
                            pluginRemarks = p.PresetParser != null ? p.PresetParser.Remarks: ""
                        }
                }
            });

            HttpContent content = new StringContent(pluginReport.ToString());

            var client = new HttpClient();

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.

            try
            {
                var response = await client.PostAsync(GetPluginReportSite(), content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _applicationService.ReportStatus("Report submitted successfully");
                    pluginsToReport.ForEach(c => c.IsReported = true);
                    _vstService.Save();

                }
                else
                {
                    _applicationService.ReportStatus("An error occured during report submission; see log for details");
                    var responseString = await response.Content.ReadAsStringAsync();
                    LogTo.Error(responseString);
                }
            }
            catch (HttpRequestException e)
            {
                _applicationService.ReportStatus("An error occured during report submission; see log for details");
                LogTo.Error(e.ToString());
            }


            base.Execute(parameter);
        }
    }
}