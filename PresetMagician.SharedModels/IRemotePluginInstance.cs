using System.Collections.Generic;
using System.Threading.Tasks;
using PresetMagician.Models;

namespace SharedModels
{
    public interface IRemotePluginInstance
    {
        Plugin Plugin { get; }

        Task LoadPlugin();

        bool OpenEditorHidden();

        void CloseEditor();

        byte[] CreateScreenshot();

        void ReloadPlugin();

        void UnloadPlugin();


        string GetPluginName();

        List<PluginInfoItem> GetPluginInfoItems();

        string GetPluginVendor();

        VstPluginInfoSurrogate GetPluginInfo();

        void SetProgram(int program);

        byte[] GetChunk(bool isPreset);

        void SetChunk(byte[] data, bool isPreset);

        string GetCurrentProgramName();

        void ExportNksAudioPreview(PresetExportInfo preset, byte[] presetData, string userContentDirectory,
            int initialDelay);

        void ExportNks(PresetExportInfo preset, byte[] presetData, string userContentDirectory);

        bool OpenEditor();
        bool IsLoaded { get; }
        bool IsEditorOpen { get; }
        string GetPluginHash();
        void KillHost();
    }
}