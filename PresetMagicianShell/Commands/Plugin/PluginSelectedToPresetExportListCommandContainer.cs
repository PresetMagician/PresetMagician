using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using System.Linq;
using Catel.Collections;
using Drachenkatze.PresetMagician.VendorPresetParser;
using MethodTimer;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
{
    // ReSharper disable once UnusedMember.Global
    public class PluginSelectedToPresetExportListCommandContainer : CommandContainerBase
    {
        private readonly IApplicationService _applicationService;

        private readonly IVstService _vstService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;

        public PluginSelectedToPresetExportListCommandContainer(ICommandManager commandManager, IVstService vstService,
            IApplicationService applicationService, IDispatcherService dispatcherService, IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.Plugin.SelectedToPresetExportList, commandManager)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => dispatcherService);
            Argument.IsNotNull(() => runtimeConfigurationService);

            _vstService = vstService;
            _applicationService = applicationService;
            _dispatcherService = dispatcherService;
            _runtimeConfigurationService = runtimeConfigurationService;

            _vstService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
            _runtimeConfigurationService.ApplicationState.PropertyChanged += OnAllowModifyPresetExportListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return (_runtimeConfigurationService.ApplicationState.AllowModifyPresetExportList &&
                    _vstService.SelectedPlugins.Count > 0);
        }

        private void OnAllowModifyPresetExportListChanged(object o, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName == nameof(ApplicationState.AllowModifyPresetExportList)) InvalidateCommand();
        }

        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            var pluginsToScan = (from plugin in _vstService.SelectedPlugins where plugin.Enabled select plugin).ToList();
            int addedPresets = 0;
            int totalPresets = 0;

            _dispatcherService.BeginInvoke(() =>
            {
                foreach (var plugin in pluginsToScan)
                {
                        var hs = new HashSet<Preset>(plugin.Presets);
                        hs.ExceptWith(_vstService.PresetExportList);

                        _vstService.PresetExportList.AddItems(hs.ToList());
                    addedPresets += hs.Count;
                    totalPresets += plugin.Presets.Count;
                }
            });

            var skippedPresets = totalPresets - addedPresets;
            string report = $"Added {addedPresets} presets from {pluginsToScan.Count} plugins.";
            if (skippedPresets > 0)
            {
                report += $" Skipped {skippedPresets} presets already on export list.";
            }
            _applicationService.ReportStatus(report);
            base.Execute(parameter);
        }
    }
}