using System;
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

        public int ProgramNumber { get; set; }

        public string getPresetHash()
        {
            return HashUtils.getFormattedSHA256Hash(this.PresetData);
        }
    }
}