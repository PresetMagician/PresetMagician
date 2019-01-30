using System;
using System.Collections.Generic;
using Anotar.Catel;
using PresetMagician.Models;
using PresetMagician.VstHost.VST;
using SharedModels;

namespace PresetMagician.ProcessIsolation.Services
{
    public class ProxiedRemoteVstService : IRemoteVstService
    {
        private IRemoteVstService _remoteVstService;
        private IIsolatedProcess _isolatedProcess;

        public ProxiedRemoteVstService(IRemoteVstService remoteVstService, IIsolatedProcess isolatedProcess)
        {
            _remoteVstService = remoteVstService;
            _isolatedProcess = isolatedProcess;
        }

        public bool Exists(string file)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: Exists({file})");
            return _remoteVstService.Exists(file);
        }

        public long GetSize(string file)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: GetSize({file})");
            return _remoteVstService.GetSize(file);
        }

        public string GetHash(string file)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: GetHash({file})");
            return _remoteVstService.GetHash(file);
        }

        public byte[] GetContents(string file)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: GetContents({file})");
            return _remoteVstService.GetContents(file);
        }

        public Guid RegisterPlugin(string dllPath, bool backgroundProcessing = true)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: RegisterPlugin({dllPath},{backgroundProcessing.ToString()})");
            return _remoteVstService.RegisterPlugin(dllPath, backgroundProcessing);
        }

        public void LoadPlugin(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: LoadPlugin({pluginGuid})");
            _remoteVstService.LoadPlugin(pluginGuid);
        }

        public bool OpenEditorHidden(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: OpenEditorHidden({pluginGuid})");
            return _remoteVstService.OpenEditorHidden(pluginGuid);
        }

        public void CloseEditor(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: CloseEditor({pluginGuid})");
            _remoteVstService.CloseEditor(pluginGuid);
        }

        public byte[] CreateScreenshot(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: CreateScreenshot({pluginGuid})");
            return _remoteVstService.CreateScreenshot(pluginGuid);
        }

        public void ReloadPlugin(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: ReloadPlugin({pluginGuid})");
            _remoteVstService.ReloadPlugin(pluginGuid);
        }

        public void UnloadPlugin(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: UnloadPlugin({pluginGuid})");
            _remoteVstService.UnloadPlugin(pluginGuid);
        }

        public bool Ping()
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: Ping()");
            return _remoteVstService.Ping();
        }

        public string GetPluginName(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: GetPluginName({pluginGuid})");
            return _remoteVstService.GetPluginName(pluginGuid);
        }

        public List<PluginInfoItem> GetPluginInfoItems(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: GetPluginInfoItems({pluginGuid})");
            return _remoteVstService.GetPluginInfoItems(pluginGuid);
        }

        public string GetPluginVendor(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: GetPluginVendor({pluginGuid})");
            return _remoteVstService.GetPluginVendor(pluginGuid);
        }

        public VstPluginInfoSurrogate GetPluginInfo(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: GetPluginInfo({pluginGuid})");
            return _remoteVstService.GetPluginInfo(pluginGuid);
        }

        public void SetProgram(Guid pluginGuid, int program)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: SetProgram({pluginGuid},{program})");
            _remoteVstService.SetProgram(pluginGuid, program);
        }

        public byte[] GetChunk(Guid pluginGuid, bool isPreset)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: GetChunk({pluginGuid},{isPreset.ToString()})");
            return _remoteVstService.GetChunk(pluginGuid, isPreset);
        }

        public void SetChunk(Guid pluginGuid, byte[] data, bool isPreset)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: SetChunk({pluginGuid},byte[] {data.Length}, {isPreset.ToString()})");
            _remoteVstService.SetChunk(pluginGuid, data, isPreset);
        }

        public string GetCurrentProgramName(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: GetCurrentProgramName({pluginGuid})");
            return _remoteVstService.GetCurrentProgramName(pluginGuid);
        }

        public void ExportNksAudioPreview(Guid pluginGuid, PresetExportInfo preset, byte[] presetData, string userContentDirectory,
            int initialDelay)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: ExportNksAudioPreview({pluginGuid})");
            _remoteVstService.ExportNksAudioPreview(pluginGuid, preset, presetData, userContentDirectory, initialDelay);
        }

        public void ExportNks(Guid pluginGuid, PresetExportInfo preset, byte[] presetData, string userContentDirectory)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: ExportNks({pluginGuid})");
            _remoteVstService.ExportNks(pluginGuid, preset, presetData, userContentDirectory);
        }

        public bool OpenEditor(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: OpenEditor({pluginGuid})");
            return _remoteVstService.OpenEditor(pluginGuid);
        }

        public string GetPluginHash(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: GetPluginHash({pluginGuid})");
            return _remoteVstService.GetPluginHash(pluginGuid);
        }

        public int GetPluginVendorVersion(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: GetPluginVendorVersion({pluginGuid})");
            return _remoteVstService.GetPluginVendorVersion(pluginGuid);
        }

        public string GetPluginProductString(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: GetPluginProductString({pluginGuid})");
            return _remoteVstService.GetPluginProductString(pluginGuid);
        }

        public string GetEffectivePluginName(Guid pluginGuid)
        {
            //LogTo.Debug($"{_isolatedProcess.Pid}: GetEffectivePluginName({pluginGuid})");
            return _remoteVstService.GetEffectivePluginName(pluginGuid);
        }

        public void KillSelf()
        {
            _remoteVstService.KillSelf();
        }
    }
}