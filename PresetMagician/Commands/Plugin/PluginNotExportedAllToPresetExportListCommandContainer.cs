using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Catel;
using Catel.Collections;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginNotExportedAllToPresetExportListCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IApplicationService _applicationService;
        private readonly IDispatcherService _dispatcherService;
        private readonly ILicenseService _licenseService;
        private readonly GlobalService _globalService;
        private readonly GlobalFrontendService _globalFrontendService;

        public PluginNotExportedAllToPresetExportListCommandContainer(ICommandManager commandManager,
            IApplicationService applicationService, IDispatcherService dispatcherService,
            IRuntimeConfigurationService runtimeConfigurationService, ILicenseService licenseService)
            : base(Commands.Plugin.NotExportedAllToPresetExportList, commandManager, runtimeConfigurationService)
        {
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => dispatcherService);
            Argument.IsNotNull(() => licenseService);

            _applicationService = applicationService;
            _dispatcherService = dispatcherService;
            _licenseService = licenseService;
            _globalService = ServiceLocator.Default.ResolveType<GlobalService>();
            _globalFrontendService = ServiceLocator.Default.ResolveType<GlobalFrontendService>();
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var pluginsToScan = (from plugin in _globalService.Plugins where plugin.IsEnabled select plugin).ToList();
            var addedPresets = 0;
            var totalPresets = 0;
            bool listExceeded = false;
            _dispatcherService.BeginInvoke(() =>
            {
                foreach (var plugin in pluginsToScan)
                {
                    var hs = new HashSet<Preset>();

                    if (_licenseService.GetPresetExportLimit() > 0)
                    {
                        hs.AddRange(plugin.Presets.Take(_licenseService.GetPresetExportLimit()));

                        if (plugin.Presets.Count > _licenseService.GetPresetExportLimit())
                        {
                            listExceeded = true;
                        }
                    }
                    else
                    {
                        hs.AddRange(plugin.Presets);
                    }

                    hs.ExceptWith(_globalFrontendService.PresetExportList);
                    hs.RemoveWhere(p => !p.IsMetadataModified);

                    _globalFrontendService.PresetExportList.AddItems(hs.ToList());

                    addedPresets += hs.Count;
                    totalPresets += plugin.Presets.Count;
                }
            });

            if (listExceeded)
            {
                MessageBox.Show(
                    $"The trial version is limited to {_licenseService.GetPresetExportLimit()} presets per plugin.");
            }

            var skippedPresets = totalPresets - addedPresets;
            var report = $"Added {addedPresets} presets from {pluginsToScan.Count} plugins.";
            if (skippedPresets > 0)
            {
                report += $" Skipped {skippedPresets} presets already on export list or already exported.";
            }

            _applicationService.ReportStatus(report);
            base.Execute(parameter);
        }
    }
}