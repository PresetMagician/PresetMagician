using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Threading;
using PresetMagician.Services.Interfaces;
using SharedModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetExportCommandContainer : CommandContainerBase
    {
        private static readonly ILog _log = LogManager.GetCurrentClassLogger();

        private readonly IVstService _vstService;
        private readonly IApplicationService _applicationService;
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;

        public PresetExportCommandContainer(ICommandManager commandManager, IVstService vstService,
            IApplicationService applicationService,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.Preset.Export, commandManager)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => runtimeConfigurationService);

            _vstService = vstService;
            _applicationService = applicationService;
            _runtimeConfigurationService = runtimeConfigurationService;
        }

        [STAThread]
        protected override async Task ExecuteAsync(object parameter)
        {
            var pluginPresets = from item in _vstService.PresetExportList
                group item by item.Plugin
                into pluginGroup
                let first = pluginGroup.First()
                select new
                {
                    Plugin = first.Plugin,
                    Presets = pluginGroup.Select(gi => new {Preset = gi})
                };

            int totalPresets = _vstService.PresetExportList.Count;
            int currentPreset = 0;

            _applicationService.StartApplicationOperation(this, "Exporting Presets",
                totalPresets);

            var cancellationToken = _applicationService.GetApplicationOperationCancellationSource().Token;

            await TaskHelper.Run(async () =>
            {
                var exportDirectory = _runtimeConfigurationService.RuntimeConfiguration
                    .NativeInstrumentsUserContentDirectory;

                if (!Directory.Exists(exportDirectory))
                {
                    _log.Warning($"Directory {exportDirectory} does not exist, using the default");
                }

                foreach (var pluginPreset in pluginPresets)
                {
                    var plugin = (from q in _vstService.Plugins
                        where q.DllPath == pluginPreset.Plugin.DllPath
                        select q).First();


                    // Create a temporary plugin so that we don't overwrite the plugin info in the list
                    var tempPlugin = new Plugin {DllPath = plugin.DllPath};

                    var remotePluginInstance = await _vstService.GetRemotePluginInstance(plugin);

                    await remotePluginInstance.LoadPlugin();

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

                        var presetData = _vstService.GetPresetData(preset.Preset);
                        var presetExportInfo = new PresetExportInfo(preset.Preset);
                        if (_runtimeConfigurationService.RuntimeConfiguration.ExportWithAudioPreviews &&
                            plugin.PluginType == Plugin.PluginTypes.Instrument)
                        {
                            remotePluginInstance.ExportNksAudioPreview(presetExportInfo, presetData, exportDirectory,
                                preset.Preset.Plugin.GetAudioPreviewDelay());
                        }

                        remotePluginInstance.ExportNks(presetExportInfo, presetData, exportDirectory);

                        plugin.PresetParser.OnAfterPresetExport();
                        preset.Preset.LastExported = DateTime.Now;
                    }

                    remotePluginInstance.UnloadPlugin();
                    await _vstService.SavePlugins();
                }
            });

            _applicationService.StopApplicationOperation("Export completed");
        }
    }
}