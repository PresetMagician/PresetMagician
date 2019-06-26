using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catel.Threading;
using Jacobi.Vst.Core;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Models.Audio;
using PresetMagician.Core.Models.MIDI;

namespace PresetMagician.RemoteVstHost
{
    public class RemotePluginInstance : IRemotePluginInstance
    {
        public Plugin Plugin { get; }
        private Guid _guid;
        public bool IsLoaded { get; private set; }
        public bool IsEditorOpen { get; private set; }
        private readonly IRemoteVstService _remoteVstService;
        private readonly IVstHostProcess _vstHostProcess;
        private readonly bool _debug;


        public RemotePluginInstance(IVstHostProcess vstHostProcess, Plugin plugin, bool backgroundProcessing = true,
            bool debug = false)
        {
            Plugin = plugin;
            _debug = debug;
            _vstHostProcess = vstHostProcess;
            _vstHostProcess.Lock(plugin);
            Plugin.Logger.Debug($"Locking to PID {vstHostProcess.Pid}");
            _remoteVstService = vstHostProcess.GetVstService();
            RegisterPlugin(backgroundProcessing);
        }

        private void RegisterPlugin(bool backgroundProcessing = true)
        {
            _guid = _remoteVstService.RegisterPlugin(Plugin.DllPath, backgroundProcessing);
        }

        public async Task LoadPlugin()
        {
            if (IsLoaded)
            {
                return;
            }

            await TaskHelper.Run(() =>
            {
                try
                {
                    _remoteVstService.LoadPlugin(_guid, _debug);
                    Plugin.PluginInfo = _remoteVstService.GetPluginInfo(_guid);
                    Plugin.VstPluginId = Plugin.PluginInfo.PluginID;
                    Plugin.PluginLocation.VstPluginId = Plugin.VstPluginId;
                    Plugin.PluginLocation.DllHash = _remoteVstService.GetHash(Plugin.DllPath);
                    Plugin.PluginLocation.LastModifiedDateTime = _remoteVstService.GetLastModifiedDate(Plugin.DllPath);
                    Plugin.PluginLocation.PluginName = _remoteVstService.GetEffectivePluginName(_guid);
                    Plugin.PluginLocation.PluginVendor = _remoteVstService.GetPluginVendor(_guid);
                    Plugin.PluginLocation.PluginProduct = _remoteVstService.GetPluginProductString(_guid);
                    Plugin.PluginLocation.VendorVersion = _remoteVstService.GetPluginVendorVersion(_guid).ToString();

                    Plugin.PluginType = Plugin.PluginInfo.Flags.HasFlag(VstPluginFlags.IsSynth)
                        ? Plugin.PluginTypes.Instrument
                        : Plugin.PluginTypes.Effect;

                    Plugin.PluginCapabilities.Clear();
                    Plugin.PluginCapabilities.AddRange(_remoteVstService.GetPluginInfoItems(_guid));
                    IsLoaded = true;
                    Plugin.PluginLocation.HasMetadata = true;
                }
                catch (Exception e)
                {
                    Plugin.LogPluginError("loading plugin", e);
                }
            }, true);
        }


        public bool OpenEditorHidden()
        {
            IsEditorOpen = _remoteVstService.OpenEditorHidden(_guid);
            return IsEditorOpen;
        }

        public void CloseEditor()
        {
            _remoteVstService.CloseEditor(_guid);
            IsEditorOpen = false;
        }

        public byte[] CreateScreenshot()
        {
            return _remoteVstService.CreateScreenshot(_guid);
        }

        public void ReloadPlugin()
        {
            _remoteVstService.ReloadPlugin(_guid);
        }

        public void PerformIdleLoop(int loops)
        {
            _remoteVstService.PerformIdleLoop(_guid, loops);
        }

        public void UnloadPlugin()
        {
            if (IsLoaded)
            {
                _remoteVstService.UnloadPlugin(_guid);
                IsLoaded = false;
            }
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

        public void ExportNksAudioPreview(PresetExportInfo preset, byte[] presetData,
            int initialDelay)
        {
            _remoteVstService.ExportNksAudioPreview(_guid, preset, presetData, initialDelay);
        }

        public void ExportNks(PresetExportInfo preset, byte[] presetData)
        {
            _remoteVstService.ExportNks(_guid, preset, presetData);
        }

        public bool OpenEditor(bool topmost = true)
        {
            IsEditorOpen = _remoteVstService.OpenEditor(_guid, topmost);
            return IsEditorOpen;
        }

        public float GetParameter(int parameterIndex)
        {
            return _remoteVstService.GetParameter(_guid, parameterIndex);
        }

        public void PatchPluginToAudioOutput(AudioOutputDevice device, int latency)
        {
            _remoteVstService.PatchPluginToAudioOutput(_guid, device, latency);
        }

        public void UnpatchPluginFromAudioOutput()
        {
            _remoteVstService.UnpatchPluginFromAudioOutput();
        }

        public void PatchPluginToMidiInput(MidiInputDevice device)
        {
            _remoteVstService.PatchPluginToMidiInput(_guid, device);
        }

        public void UnpatchPluginFromMidiInput()
        {
            _remoteVstService.UnpatchPluginFromMidiInput();
        }

        public void DisableTimeInfo()
        {
            _remoteVstService.DisableTimeInfo(_guid);
        }

        public void EnableTimeInfo()
        {
            _remoteVstService.EnableTimeInfo(_guid);
        }

        public void Dispose()
        {
            if (_vstHostProcess.IsRemoteVstServiceAvailable())
            {
                _remoteVstService.UnregisterPlugin(_guid);
            }

            Plugin.Logger.Debug($"Unlocking from PID {_vstHostProcess.Pid}");
            _vstHostProcess.Unlock();
        }
    }
}