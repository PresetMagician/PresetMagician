using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Catel;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagicianShell.Helpers;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;
using PresetMagicianShell.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
{
    // ReSharper disable once UnusedMember.Global
    public class PresetExportCommandContainer : CommandContainerBase
    {
        private static readonly ILog _log = LogManager.GetCurrentClassLogger();

        private readonly IVstService _vstService;
        private readonly IApplicationService _applicationService;

        public PresetExportCommandContainer(ICommandManager commandManager, IVstService vstService,IApplicationService applicationService)
            : base(Commands.Preset.Export, commandManager)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => applicationService);

            _vstService = vstService;
            _applicationService = applicationService;
        }

        [STAThread]
        protected override async Task ExecuteAsync(object parameter)
        {
            var pluginPresets = from item in _vstService.PresetExportList
                group item by item.PluginDLLPath into pluginGroup
                let first = pluginGroup.First()
                select new
                {
                    Plugin = new { DLLPath = first.PluginDLLPath },
                    Presets = pluginGroup.Select(gi => new { Preset = gi })
                };

            int totalPresets = _vstService.PresetExportList.Count;
            int currentPreset = 0;

            _applicationService.StartApplicationOperation(this, "Exporting Presets",
                totalPresets);

            var cancellationToken = _applicationService.GetApplicationOperationCancellationSource().Token;

            await TaskHelper.Run(() =>
            {
                var exporter = new VstPluginExport(_vstService.VstHost);

                foreach (var pluginPreset in pluginPresets)
                {
                    var plugin = (from q in _vstService.Plugins
                        where q.DllPath == pluginPreset.Plugin.DLLPath
                        select q).First();


                    // Create a temporary plugin so that we don't overwrite the plugin info in the list
                    var tempPlugin = new Plugin();
                    tempPlugin.DllPath = plugin.DllPath;

                    Debug.WriteLine("Loading plugin "+plugin.DllPath);


                    _vstService.VstHost.LoadVST(tempPlugin);

                    foreach (var preset in pluginPreset.Presets)
                    {
                        currentPreset++;
                        _applicationService.UpdateApplicationOperationStatus(
                            currentPreset,
                            $"Exporting {plugin.PluginName} - {preset.Preset.PresetName}");

                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                        //currentPreset++;
                        exporter.ExportPresetAudioPreviewRealtime(tempPlugin, preset.Preset);
                        exporter.ExportPresetNKSF(tempPlugin, preset.Preset);
                        plugin.PresetParser.OnAfterPresetExport(_vstService.VstHost, tempPlugin);
                        //worker.ReportProgress((int)((100f * currentPreset) / presets.Count), preset.Preset.PluginName + " " + preset.Preset.PresetName);
                    }

                    _vstService.VstHost.UnloadVST(tempPlugin);
                }
            });

            _applicationService.StopApplicationOperation("Export completed");
       
        }
    }
}   