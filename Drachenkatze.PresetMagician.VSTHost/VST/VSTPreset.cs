using System;
using System.ComponentModel;
using CannedBytes.Midi.Message;
using Drachenkatze.PresetMagician.Controls.Controls.VSTHost;
using Drachenkatze.PresetMagician.Utils;

namespace Drachenkatze.PresetMagician.VSTHost.VST
{
    public class VSTPreset : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public VSTPreset()
        {
        }

        public string getPresetHash ()
        {
            return HashUtils.getFormattedSHA256Hash(this.PresetData);
        }

        public String PresetName
        {
            get; set;
        }

        public int ProgramNumber
        {
            get; set;
        }

        public MidiNoteName PreviewNote
        {
            get; set;
        }

        public bool Export
        {
            get; set;
        }

        public String BankName
        {
            get; set;
        }

        public String PluginName
        {
            get; set;
        }

        public String PluginDLLPath;

        public byte[] PresetData
        {
            get; set;
        }

        public String NKSFBankName
        {
            get
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
        }

        public String NKSFPluginName
        {
            get
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
        }

        public String NKSFPresetName
        {
            // Returns the sanitized preset name
            get
            {
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
        }

    }
}