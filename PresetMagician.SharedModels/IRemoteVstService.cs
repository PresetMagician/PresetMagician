using System;
using System.Collections.Generic;
using System.ServiceModel;
using PresetMagician.Models;

namespace SharedModels
{
    [ServiceContract(Namespace = "https://presetmagician.com")]
    public interface IRemoteVstService
    {
        [OperationContract]
        Guid RegisterPlugin(string dllPath, bool backgroundProcessing = true);

        [OperationContract]
        void LoadPlugin(Guid guid);

        [OperationContract]
        bool OpenEditorHidden(Guid pluginGuid);

        [OperationContract]
        void CloseEditor(Guid pluginGuid);

        [OperationContract]
        byte[] CreateScreenshot(Guid pluginGuid);

        [OperationContract]
        void ReloadPlugin(Guid guid);

        [OperationContract]
        void UnloadPlugin(Guid guid);

        [OperationContract]
        bool Ping();

        [OperationContract]
        string GetPluginName(Guid pluginGuid);

        [OperationContract]
        List<PluginInfoItem> GetPluginInfoItems(Guid pluginGuid);

        [OperationContract]
        string GetPluginVendor(Guid pluginGuid);

        [OperationContract]
        VstPluginInfoSurrogate GetPluginInfo(Guid pluginGuid);

        [OperationContract]
        void SetProgram(Guid pluginGuid, int program);

        [OperationContract]
        byte[] GetChunk(Guid pluginGuid, bool isPreset);

        [OperationContract]
        void SetChunk(Guid pluginGuid, byte[] data, bool isPreset);

        [OperationContract]
        string GetCurrentProgramName(Guid pluginGuid);

        [OperationContract]
        void ExportNksAudioPreview(Guid pluginGuid, PresetExportInfo preset, byte[] presetData,
            string userContentDirectory, int initialDelay);

        [OperationContract]
        void ExportNks(Guid pluginGuid, PresetExportInfo preset, byte[] presetData, string userContentDirectory);

        [OperationContract]
        bool OpenEditor(Guid pluginGuid);

        [OperationContract]
        string GetPluginHash(Guid guid);
    }
}