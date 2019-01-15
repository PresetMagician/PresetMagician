using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Catel;
using Catel.Collections;
using Catel.MVVM;
using Catel.Services;
using Drachenkatze.PresetMagician.VendorPresetParser;
using PresetMagician.Models;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using SharedModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginAllToPresetExportListCommandContainer : CommandContainerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private readonly ILicenseService _licenseService;
        private readonly IVstService _vstService;

        public PluginAllToPresetExportListCommandContainer(ICommandManager commandManager, IVstService vstService,
            IApplicationService applicationService, IDispatcherService dispatcherService,
            IRuntimeConfigurationService runtimeConfigurationService, ILicenseService licenseService)
            : base(Commands.Plugin.AllToPresetExportList, commandManager)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => dispatcherService);
            Argument.IsNotNull(() => runtimeConfigurationService);
            Argument.IsNotNull(() => licenseService);

            _vstService = vstService;
            _applicationService = applicationService;
            _dispatcherService = dispatcherService;
            _runtimeConfigurationService = runtimeConfigurationService;
            _licenseService = licenseService;

            _runtimeConfigurationService.ApplicationState.PropertyChanged += OnAllowModifyPresetExportListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return _runtimeConfigurationService.ApplicationState.AllowModifyPresetExportList;
        }

        private void OnAllowModifyPresetExportListChanged(object o, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName == nameof(ApplicationState.AllowModifyPresetExportList))
            {
                InvalidateCommand();
            }
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            var pluginsToScan = (from plugin in _vstService.Plugins where plugin.IsEnabled select plugin).ToList();
            var addedPresets = 0;
            var totalPresets = 0;
            bool listExceeded = false;
            _dispatcherService.BeginInvoke(() =>
            {
                foreach (var plugin in pluginsToScan)
                {
                    var hs = new HashSet<Preset>();

                    if (_licenseService.getPresetExportLimit() > 0)
                    {
                        hs.AddRange(plugin.Presets.Take(_licenseService.getPresetExportLimit()));

                        if (plugin.Presets.Count > _licenseService.getPresetExportLimit())
                        {
                            listExceeded = true;
                        }
                    }
                    else
                    {
                        hs.AddRange(plugin.Presets);
                    }

                    hs.ExceptWith(_vstService.PresetExportList);

                    _vstService.PresetExportList.AddItems(hs.ToList());
                    
                    addedPresets += hs.Count;
                    totalPresets += plugin.Presets.Count;
                }
            });
            
            if (listExceeded)
            {
                MessageBox.Show(
                    $"The trial version is limited to {_licenseService.getPresetExportLimit()} presets per plugin.");
            }

            var skippedPresets = totalPresets - addedPresets;
            var report = $"Added {addedPresets} presets from {pluginsToScan.Count} plugins.";
            if (skippedPresets > 0)
            {
                report += $" Skipped {skippedPresets} presets already on export list.";
            }

            _applicationService.ReportStatus(report);
            base.Execute(parameter);
        }
    }
}