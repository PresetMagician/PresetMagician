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
using Orchestra;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services;
using PresetMagicianShell.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
{
    // ReSharper disable once UnusedMember.Global
    public abstract class AbstractReportPluginsCommandContainer : CommandContainerBase
    {
        protected abstract ILog _log { get; set; }
        private readonly IApplicationService _applicationService;
        private readonly ILicenseService _licenseService;
        private readonly IVstService _vstService;
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        protected bool ReportAll { get; set; } = false;

        protected AbstractReportPluginsCommandContainer(string command, ICommandManager commandManager, IVstService vstService,
            ILicenseService licenseService, IApplicationService applicationService, IRuntimeConfigurationService runtimeConfigurationService)
            : base(command, commandManager)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => licenseService);
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => runtimeConfigurationService);

            _vstService = vstService;
            _licenseService = licenseService;
            _applicationService = applicationService;
            _runtimeConfigurationService = runtimeConfigurationService;

            _runtimeConfigurationService.ApplicationState.PropertyChanged += OnAllowReportUnsupportedPluginsChanged;

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
                    where plugin.IsScanned
                    select plugin).Count();
            }
            else
            {
                numPluginsToReport = (from plugin in _vstService.Plugins
                    where plugin.IsScanned && plugin.IsSupported == false
                    select plugin).Count();
               
            }

            return (
                _runtimeConfigurationService.ApplicationState.AllowReportUnsupportedPlugins && numPluginsToReport > 0);

        }

        private void OnPluginListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
           
                InvalidateCommand();
            
        }

        private void OnAllowReportUnsupportedPluginsChanged(object o, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName == nameof(ApplicationState.AllowReportUnsupportedPlugins))
            {
                InvalidateCommand();
            }
        }

        private void OnPluginItemPropertyChanged(object o, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName == nameof(Plugin.IsSupported))
            {
                InvalidateCommand();
            }
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            List<Plugin> pluginsToReport;

            if (ReportAll)
            {
                pluginsToReport = (from plugin in _vstService.Plugins
                    where plugin.IsScanned
                    select plugin).ToList();
            }
            else
            {
                pluginsToReport = (from plugin in _vstService.Plugins
                    where plugin.IsScanned && plugin.IsSupported == false
                    select plugin).ToList();
            }
            
            var pluginReport = JObject.FromObject(new
            {
                pluginSubmission = new
                {
                    email = _licenseService.GetCurrentLicense().Customer.Email,
                    plugins =
                        from p in pluginsToReport
                        orderby p.PluginId
                        select new
                        {
                            vendorName = p.PluginVendor,
                            pluginName = p.PluginName,
                            pluginId = p.PluginId,
                            pluginSupported = p.IsSupported,
                            pluginSupportedSince = VersionHelper.GetCurrentVersion(),
                            pluginType = p.PluginTypeDescription,
                            pluginCapabilities = p.PluginInfoItems
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
                var response = await client.PostAsync(Settings.Links.SubmitPlugins, content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _applicationService.ReportStatus("Report submitted successfully");
                }
                else
                {
                    _applicationService.ReportStatus("An error occured during report submission; see log for details");
                    var responseString = await response.Content.ReadAsStringAsync();
                    _log.Error(responseString);
                }
            }
            catch (HttpRequestException e)
            {
                _applicationService.ReportStatus("An error occured during report submission; see log for details");
                _log.Error(e.ToString());
            }


            base.Execute(parameter);
        }
    }
}