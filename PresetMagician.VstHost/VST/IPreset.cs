using System;
using System.Collections.ObjectModel;
using CannedBytes.Midi.Message;

namespace Drachenkatze.PresetMagician.VSTHost.VST
{
    public interface IPreset
    {
        IPresetBank PresetBank { get; set; }
        bool Export { get; set; }
        String PluginName { get; set; }
        byte[] PresetData { get; set; }
        String PresetName { get; set; }
        MidiNoteName PreviewNote { get; set; }
        int ProgramNumber { get; set; }
        ObservableCollection<ObservableCollection<string>> Types { get; set; }
        ObservableCollection<string> Modes { get; set; }
        string Author { get; set; }
        string Comment { get; set; }
    }
}