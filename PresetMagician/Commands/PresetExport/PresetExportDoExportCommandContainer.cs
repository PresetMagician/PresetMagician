using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.Threading;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetExportDoExportCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IApplicationService _applicationService;
        private readonly PresetDataPersisterService _presetDataPersisterService;
        private readonly DataPersisterService _dataPersisterService;
        private readonly RemoteVstService _remoteVstService;
        private readonly GlobalService _globalService;

        public PresetExportDoExportCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator)
            : base(Commands.PresetExport.DoExport, commandManager, serviceLocator)
        {
            _presetDataPersisterService = ServiceLocator.ResolveType<PresetDataPersisterService>();
            _applicationService = ServiceLocator.ResolveType<IApplicationService>();
            _remoteVstService = ServiceLocator.ResolveType<RemoteVstService>();
            _dataPersisterService = ServiceLocator.ResolveType<DataPersisterService>();
            _globalService = ServiceLocator.ResolveType<GlobalService>();
        }

        [STAThread]
        protected override async Task ExecuteAsync(object parameter)
        {
            var pluginPresets = from item in _globalFrontendService.PresetExportList
                group item by item.Plugin
                into pluginGroup
                let first = pluginGroup.First()
                select new
                {
                    first.Plugin,
                    Presets = pluginGroup.Select(gi => new {Preset = gi})
                };

            int totalPresets = _globalFrontendService.PresetExportList.Count;
            int currentPreset = 0;

            _applicationService.StartApplicationOperation(this, "Exporting Presets",
                totalPresets);
            var progress = _applicationService.GetApplicationProgress();

            await TaskHelper.Run(async () =>
            {
                var exportDirectory = _globalService.RuntimeConfiguration
                    .NativeInstrumentsUserContentDirectory;

                if (!Directory.Exists(exportDirectory))
                {
                    LogTo.Warning($"Directory {exportDirectory} does not exist, using the default");
                }

                foreach (var pluginPreset in pluginPresets)
                {
                    try
                    {
                        await _presetDataPersisterService.OpenDatabase();

                        if (pluginPreset.Plugin.PresetParser == null)
                        {
                            _applicationService.AddApplicationOperationError(
                                $"Unable to update export presets for {pluginPreset.Plugin.PluginName} because it has no Preset Parser. Try to force-reload metadata for this plugin.");
                            continue;
                        }

                        using (var remotePluginInstance =
                            _remoteVstService.GetRemotePluginInstance(pluginPreset.Plugin, false))
                        {
                            foreach (var preset in pluginPreset.Presets)
                            {
                                currentPreset++;
                                _applicationService.UpdateApplicationOperationStatus(
                                    currentPreset,
                                    $"Exporting {pluginPreset.Plugin.PluginName} - {preset.Preset.Metadata.PresetName}");

                                if (progress.CancellationToken.IsCancellationRequested)
                                {
                                    return;
                                }

                                var presetData = await _presetDataPersisterService.GetPresetData(preset.Preset);

                                if (preset.Preset.PresetBank == null)
                                {
                                    preset.Preset.PresetBank = preset.Preset.Plugin.RootBank;
                                    LogTo.Warning(
                                        $"Preset {preset.Preset.Metadata.PresetName} has no preset bank, using none.");
                                }

                                var presetExportInfo = new PresetExportInfo(preset.Preset)
                                {
                                    FolderMode = _globalService.RuntimeConfiguration.FolderExportMode,
                                    OverwriteMode = _globalService.RuntimeConfiguration.FileOverwriteMode,
                                    UserContentDirectory = exportDirectory
                                };

                                if (!presetExportInfo.CanExport())
                                {
                                    _applicationService.AddApplicationOperationError(
                                        $"Cannot export {preset.Preset.Plugin} -{preset.Preset.Metadata.PresetName}. " +
                                        $"Reason: {presetExportInfo.CannotExportReason}");
                                    continue;
                                }

                                pluginPreset.Plugin.PresetParser.PluginInstance = remotePluginInstance;

                                if (_globalService.RuntimeConfiguration.ExportWithAudioPreviews &&
                                    pluginPreset.Plugin.PluginType == Plugin.PluginTypes.Instrument)
                                {
                                    await remotePluginInstance.LoadPlugin().ConfigureAwait(false);
                                    pluginPreset.Plugin.PresetParser.OnPluginLoad();
                                    remotePluginInstance.SetChunk(presetData, false);

                                    remotePluginInstance.ExportNksAudioPreview(presetExportInfo, presetData,
                                        preset.Preset.Plugin.GetAudioPreviewDelay());
                                }

                                remotePluginInstance.ExportNks(presetExportInfo, presetData);


                                pluginPreset.Plugin.PresetParser.OnAfterPresetExport();
                                preset.Preset.LastExported = DateTime.Now;
                                preset.Preset.UpdateLastExportedMetadata();
                            }

                            if (remotePluginInstance.IsLoaded)
                            {
                                pluginPreset.Plugin.PresetParser.OnPluginUnload();
                            }

                            remotePluginInstance.UnloadPlugin();
                        }
                    }
                    catch (Exception e)
                    {
                        _applicationService.AddApplicationOperationError(
                            $"Unable to update export presets for {pluginPreset.Plugin.PluginName} because of {e.GetType().FullName}: {e.Message}");
                        LogTo.Debug(e.StackTrace);
                    }

                    await _presetDataPersisterService.CloseDatabase();

                    _dataPersisterService.SavePresetsForPlugin(pluginPreset.Plugin);
                }
            });

            _applicationService.StopApplicationOperation("Export completed");
        }
    }
}