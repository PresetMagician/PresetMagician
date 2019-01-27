using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jacobi.Vst.Core;
using Newtonsoft.Json;
using PresetMagician.Models;
using SharedModels;

namespace PresetMagician.ProcessIsolation
{
    public class RemotePluginInstance : IRemotePluginInstance
    {
        public Plugin Plugin { get; }
        private Guid _guid;
        public bool IsLoaded { get; private set; }
        private readonly IRemoteVstService _remoteVstService;
        private readonly IIsolatedProcess _isolatedProcess;

        public RemotePluginInstance(IIsolatedProcess isolatedProcess, Plugin plugin, bool backgroundProcessing = true)
        {
            Plugin = plugin;
            _isolatedProcess = isolatedProcess;
            _remoteVstService = isolatedProcess.GetVstService();
            RegisterPlugin(backgroundProcessing);
        }

        private void RegisterPlugin(bool backgroundProcessing = true)
        {
            _guid = _remoteVstService.RegisterPlugin(Plugin.DllPath, backgroundProcessing);
        }

        public async Task LoadPlugin()
        {
            try
            {
                _remoteVstService.LoadPlugin(_guid);
                Plugin.PluginName = _remoteVstService.GetPluginName(_guid);
                Plugin.PluginVendor = _remoteVstService.GetPluginVendor(_guid);
                Plugin.PluginInfo = _remoteVstService.GetPluginInfo(_guid);
                Plugin.PluginInfo.Flags =
                    JsonConvert.DeserializeObject<VstPluginFlags>(Plugin.PluginInfo.StringFlags);
                Plugin.PluginId = Plugin.PluginInfo.PluginID;
                Plugin.DllHash = _remoteVstService.GetPluginHash(_guid);

                Plugin.PluginType = Plugin.PluginInfo.Flags.HasFlag(VstPluginFlags.IsSynth)
                    ? Plugin.PluginTypes.Instrument
                    : Plugin.PluginTypes.Effect;

                Plugin.PluginInfoItems.Clear();
                Plugin.PluginInfoItems.AddRange(_remoteVstService.GetPluginInfoItems(_guid));
                IsLoaded = true;
            }
            catch (Exception e)
            {
                Plugin.OnLoadError(e);
            }
        }

        public string GetPluginHash()
        {
            return _remoteVstService.GetPluginHash(_guid);
        }

        public bool OpenEditorHidden()
        {
            return _remoteVstService.OpenEditorHidden(_guid);
        }

        public void CloseEditor()
        {
            _remoteVstService.CloseEditor(_guid);
        }

        public byte[] CreateScreenshot()
        {
            return _remoteVstService.CreateScreenshot(_guid);
        }

        public void ReloadPlugin()
        {
            _remoteVstService.ReloadPlugin(_guid);
        }

        public void UnloadPlugin()
        {
            _remoteVstService.UnloadPlugin(_guid);
            IsLoaded = false;
        }

        public string GetPluginName()
        {
            return _remoteVstService.GetPluginName(_guid);
        }

        public List<PluginInfoItem> GetPluginInfoItems()
        {
            return _remoteVstService.GetPluginInfoItems(_guid);
        }

        public string GetPluginVendor()
        {
            return _remoteVstService.GetPluginVendor(_guid);
        }

        public VstPluginInfoSurrogate GetPluginInfo()
        {
            return _remoteVstService.GetPluginInfo(_guid);
        }

        public void SetProgram(int program)
        {
            _remoteVstService.SetProgram(_guid, program);
        }

        public byte[] GetChunk(bool isPreset)
        {
            return _remoteVstService.GetChunk(_guid, isPreset);
        }

        public void SetChunk(byte[] data, bool isPreset)
        {
            _remoteVstService.SetChunk(_guid, data, isPreset);
        }

        public string GetCurrentProgramName()
        {
            return _remoteVstService.GetCurrentProgramName(_guid);
        }

        public void ExportNksAudioPreview(PresetExportInfo preset, byte[] presetData, string userContentDirectory,
            int initialDelay)
        {
            _remoteVstService.ExportNksAudioPreview(_guid, preset, presetData, userContentDirectory, initialDelay);
        }

        public void ExportNks(PresetExportInfo preset, byte[] presetData, string userContentDirectory)
        {
            _remoteVstService.ExportNks(_guid, preset, presetData, userContentDirectory);
        }

        public bool OpenEditor()
        {
            return _remoteVstService.OpenEditor(_guid);
        }

        public void KillHost()
        {
            _isolatedProcess.Kill();
        }
    }
}