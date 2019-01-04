using System;
using System.Collections.ObjectModel;
using CannedBytes.Midi.Message;
using Catel.Data;
using Drachenkatze.PresetMagician.Utils;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    

    public class Preset : ModelBase, IPreset
    {
        public String PluginDLLPath;

        public void SetPlugin (IVstPlugin vst)
        {
            PluginName = vst.PluginName;
            PluginDLLPath = vst.DllPath;
        }

        public Preset()
        {
            PreviewNote = new MidiNoteName("C5");
        }

        public IPresetBank PresetBank { get; set; }

        public bool Export { get; set; } = true;

        public String PluginName { get; set; }

        public byte[] PresetData { get; set; }

        public String PresetName { get; set; }

        public MidiNoteName PreviewNote { get; set; }

        public string Author { get;set; }
        public string Comment { get;set; }

        public ObservableCollection<ObservableCollection<string>> Types { get; set; } = new ObservableCollection<ObservableCollection<string>>();

        public ObservableCollection<string> Modes { get; set; } = new ObservableCollection<string>();

        public int ProgramNumber { get; set; }

        public string getPresetHash()
        {
            return HashUtils.getFormattedSHA256Hash(PresetData);
        }
    }
}