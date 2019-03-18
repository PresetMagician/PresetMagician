using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Catel;
using Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.Threading;
using PresetMagician.Core.Interfaces;
using PresetMagician.Services.Interfaces;
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
        private readonly GlobalFrontendService _globalFrontendService;
        
        public PresetExportDoExportCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.PresetExport.DoExport, commandManager, runtimeConfigurationService)
        {
            
            _globalFrontendService = ServiceLocator.Default.ResolveType<GlobalFrontendService>();
            _presetDataPersisterService = ServiceLocator.Default.ResolveType<PresetDataPersisterService>();
            _applicationService = ServiceLocator.Default.ResolveType<IApplicationService>();
            _remoteVstService = ServiceLocator.Default.ResolveType<RemoteVstService>();
            _dataPersisterService = ServiceLocator.Default.ResolveType<DataPersisterService>();
            
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

            var cancellationToken = _applicationService.GetApplicationOperationCancellationSource().Token;

            await TaskHelper.Run(async () =>
            {
                var exportDirectory = _runtimeConfigurationService.RuntimeConfiguration
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
                        using (var remotePluginInstance =
                            _remoteVstService.GetRemotePluginInstance(pluginPreset.Plugin, false))
                        {
                            foreach (var preset in pluginPreset.Presets)
                            {
                                currentPreset++;
                                _applicationService.UpdateApplicationOperationStatus(
                                    currentPreset,
                                    $"Exporting {pluginPreset.Plugin.PluginName} - {preset.Preset.Metadata.PresetName}");

                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return;
                                }

                                var presetData = _presetDataPersisterService.GetPresetData(preset.Preset);
                                var presetExportInfo = new PresetExportInfo(preset.Preset);
                                if (_runtimeConfigurationService.RuntimeConfiguration.ExportWithAudioPreviews &&
                                    pluginPreset.Plugin.PluginType == Plugin.PluginTypes.Instrument)
                                {
                                    await remotePluginInstance.LoadPlugin().ConfigureAwait(false);
                                    remotePluginInstance.ExportNksAudioPreview(presetExportInfo, presetData,
                                        exportDirectory,
                                        preset.Preset.Plugin.GetAudioPreviewDelay());
                                }

                                remotePluginInstance.ExportNks(presetExportInfo, presetData, exportDirectory);

                                pluginPreset.Plugin.PresetParser.PluginInstance = remotePluginInstance;
                                pluginPreset.Plugin.PresetParser.OnAfterPresetExport();
                                preset.Preset.LastExported = DateTime.Now;
                                preset.Preset.UpdateLastExportedMetadata();
                            }

                            remotePluginInstance.UnloadPlugin();
                        }
                    }
                    catch (Exception e)
                    {
                        _applicationService.AddApplicationOperationError(
                            $"Unable to update export presets because of {e.GetType().FullName}: {e.Message}");
                        LogTo.Debug(e.StackTrace);
                    }
                    
                    await _presetDataPersisterService.CloseDatabase();

                    _dataPersisterService.Save();
                }
            });

            _applicationService.StopApplicationOperation("Export completed");
        }
    }
}