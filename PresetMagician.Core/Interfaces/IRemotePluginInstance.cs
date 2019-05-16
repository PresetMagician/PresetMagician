using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PresetMagician.Core.Models;
using PresetMagician.Core.Models.Audio;
using PresetMagician.Core.Models.MIDI;

namespace PresetMagician.Core.Interfaces
{
    public interface IRemotePluginInstance : IDisposable
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

        void ExportNksAudioPreview(PresetExportInfo preset, byte[] presetData,
            int initialDelay);

        void ExportNks(PresetExportInfo preset, byte[] presetData);

        void PatchPluginToAudioOutput(AudioOutputDevice device);
        void UnpatchPluginFromAudioOutput();
        void PatchPluginToMidiInput(MidiInputDevice device);
        void UnpatchPluginFromMidiInput();

        bool OpenEditor(bool topmost = true);
        bool IsLoaded { get; }
        bool IsEditorOpen { get; }
        float GetParameter(int parameterIndex);
    }
}