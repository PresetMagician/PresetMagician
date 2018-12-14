using System;
using System.ComponentModel;
using CannedBytes.Midi.Message;
using Catel.Data;
using Drachenkatze.PresetMagician.Controls.Controls.VSTHost;
using Drachenkatze.PresetMagician.Utils;

namespace Drachenkatze.PresetMagician.VSTHost.VST
{
    public class Preset : ModelBase
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

        public String BankName { get; set; }

        public bool Export { get; set; } = true;

        public String getNKSFBankName()
        {
                String bankName = BankName;

                foreach (char c in System.IO.Path.GetInvalidPathChars())
                {
                    bankName = bankName.Replace(c, '_');
                }

                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    bankName = bankName.Replace(c, '_');
                }

                return bankName;

        }

        public String getNKSFPluginName ()
        {
                String pluginName = PluginName;

                foreach (char c in System.IO.Path.GetInvalidPathChars())
                {
                    pluginName = pluginName.Replace(c, '_');
                }

                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    pluginName = pluginName.Replace(c, '_');
                }

                return pluginName;
        }

        public String getNKSFPresetName () 
        {
            // Returns the sanitized preset name
                String presetName = PresetName;

                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    presetName = presetName.Replace(c, '_');
                }

                foreach (char c in System.IO.Path.GetInvalidPathChars())
                {
                    presetName = presetName.Replace(c, '_');
                }

                return presetName;
        }

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