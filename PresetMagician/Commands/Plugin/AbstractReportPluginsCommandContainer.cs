﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Anotar.Catel;
using Catel.Data;
using Catel.IoC;
using Catel.MVVM;
using Newtonsoft.Json.Linq;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public abstract class AbstractReportPluginsCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IApplicationService _applicationService;
        private readonly ILicenseService _licenseService;
        protected bool ReportAll;
        protected readonly GlobalService GlobalService;
        protected readonly GlobalFrontendService GlobalFrontendService;
        private readonly DataPersisterService _dataPersisterService;

        protected AbstractReportPluginsCommandContainer(string command, ICommandManager commandManager,
            IServiceLocator serviceLocator)
            : base(command, commandManager, serviceLocator)
        {
            _licenseService = serviceLocator.ResolveType<ILicenseService>();
            _applicationService = serviceLocator.ResolveType<IApplicationService>();
            _dataPersisterService = serviceLocator.ResolveType<DataPersisterService>();
            GlobalService = serviceLocator.ResolveType<GlobalService>();
            GlobalFrontendService = Catel.IoC.ServiceLocator.Default.ResolveType<GlobalFrontendService>();

            var wrapper = new ChangeNotificationWrapper(GlobalService.Plugins);
            wrapper.CollectionItemPropertyChanged += OnPluginItemPropertyChanged;
            wrapper.CollectionChanged += OnPluginListChanged;

            ReportAll = false;
        }

        protected override bool CanExecute(object parameter)
        {
            int numPluginsToReport;

            if (ReportAll)
            {
                numPluginsToReport = (from plugin in GlobalService.Plugins
                    where plugin.HasMetadata && !plugin.DontReport && !plugin.IsReported
                    select plugin).Count();
            }
            else
            {
                numPluginsToReport = (from plugin in GlobalService.Plugins
                    where plugin.HasMetadata && plugin.IsSupported == false && !plugin.DontReport && !plugin.IsReported
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
                return (from plugin in GlobalService.Plugins
                    where plugin.HasMetadata && !plugin.DontReport && !plugin.IsReported
                    select plugin).ToList();
            }


            return (from plugin in GlobalService.Plugins
                where plugin.HasMetadata && plugin.IsSupported == false && !plugin.DontReport && !plugin.IsReported
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
                        where p.PluginLocation != null
                        orderby p.VstPluginId
                        select new
                        {
                            vendorName = p.PluginVendor,
                            pluginName = p.PluginName,
                            pluginId = p.VstPluginId,
                            pluginSupported = p.IsSupported,
                            pluginPresetParser = p.PresetParser != null ? p.PresetParser.PresetParserType : "",
                            pluginSupportedSince = GlobalService.PresetMagicianVersion,
                            pluginType = p.PluginTypeDescription,
                            dllHash = p.PluginLocation.DllHash,
                            pluginCapabilities = p.PluginCapabilities,
                            pluginRemarks = p.PresetParser != null ? p.PresetParser.Remarks : ""
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
                    _dataPersisterService.Save();
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