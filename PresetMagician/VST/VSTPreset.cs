using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CannedBytes.Midi.Message;
using System.Diagnostics;

namespace PresetMagician.VST
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

        public String PresetData
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