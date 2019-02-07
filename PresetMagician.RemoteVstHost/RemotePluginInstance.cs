using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catel.Logging;
using Catel.Threading;
using Jacobi.Vst.Core;
using PresetMagician.Models;
using PresetMagician.RemoteVstHost.Processes;
using SharedModels;

namespace PresetMagician.RemoteVstHost
{
    public class RemotePluginInstance : IRemotePluginInstance
    {
        public Plugin Plugin { get; }
        private Guid _guid;
        public bool IsLoaded { get; private set; }
        public bool IsEditorOpen { get; private set; }
        private readonly IRemoteVstService _remoteVstService;
        private readonly VstHostProcess _vstHostProcess;
        private readonly bool _debug;
        

        public RemotePluginInstance(VstHostProcess vstHostProcess, Plugin plugin, bool backgroundProcessing = true, bool debug=false)
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
                    Plugin.PluginName = _remoteVstService.GetEffectivePluginName(_guid);
                    Plugin.PluginVendor = _remoteVstService.GetPluginVendor(_guid);
                    Plugin.PluginInfo = _remoteVstService.GetPluginInfo(_guid);
                    Plugin.VstPluginId = Plugin.PluginInfo.PluginID;
                    Plugin.PluginLocation.VstPluginId = Plugin.VstPluginId;
                    Plugin.PluginLocation.DllHash = _remoteVstService.GetHash(Plugin.DllPath);
                    Plugin.PluginLocation.LastModifiedDateTime = _remoteVstService.GetLastModifiedDate(Plugin.DllPath);
                    Plugin.PluginLocation.PluginName = _remoteVstService.GetPluginName(_guid);
                    Plugin.PluginLocation.PluginVendor = Plugin.PluginVendor;
                    Plugin.PluginLocation.PluginProduct = _remoteVstService.GetPluginProductString(_guid);
                    Plugin.PluginLocation.VendorVersion = _remoteVstService.GetPluginVendorVersion(_guid).ToString();


                    Plugin.PluginType = Plugin.PluginInfo.Flags.HasFlag(VstPluginFlags.IsSynth)
                        ? Plugin.PluginTypes.Instrument
                        : Plugin.PluginTypes.Effect;

                    Plugin.PluginCapabilities.Clear();
                    Plugin.PluginCapabilities.AddRange(_remoteVstService.GetPluginInfoItems(_guid));
                    IsLoaded = true;
                    Plugin.HasMetadata = true;
                }
                catch (Exception e)
                {
                    Plugin.OnLoadError(e);
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
            IsEditorOpen = _remoteVstService.OpenEditor(_guid);
            return IsEditorOpen;
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