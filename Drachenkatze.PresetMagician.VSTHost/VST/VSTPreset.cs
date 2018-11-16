using System;
using System.ComponentModel;
using CannedBytes.Midi.Message;
using Drachenkatze.PresetMagician.Controls.Controls.VSTHost;

namespace Drachenkatze.PresetMagician.VSTHost.VST
{
    public class VSTPreset : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public VSTPreset()
        {
        }

        public String PresetName
        {
            get; set;
        }

        public int ProgramNumber
        {
            get; set;
        }

        public VSTPlugin VstPlugin
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

        public byte[] PresetData
        {
            get; set;
        }

        public String NKSFBankName
        {
            get
            {
                return BankName;
            }
        }

        public String NKSFPluginName
        {
            get
            {
                return VstPlugin.PluginName;
            }
        }

        public String NKSFPresetName
        {
            get
            {
                return PresetName;
            }
        }
    }
}