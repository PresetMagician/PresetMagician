using Drachenkatze.PresetMagician.VSTHost.VST;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public abstract class AbstractVendorPresetParser : INotifyPropertyChanged
    {
        public IVstPlugin VstPlugin { get; set; }
        public virtual List<int> SupportedPlugins => new List<int>();

        public virtual string Remarks { get; set; }

        public bool IsNullParser => false;

        public string PresetParserType => GetType().Name.ToString();

        public int NumPresets
        {
            get
            {
                int totalPresets = 0;
                foreach (var bank in Banks)
                {
                    totalPresets += bank.VSTPresets.Count;
                }

                return totalPresets;
            }
        }

        public virtual List<PresetBank> Banks { get; set; } = new List<PresetBank>();

        public virtual bool CanHandle()
        {
            if (SupportedPlugins.Contains(VstPlugin.PluginId))
            {
                return true;
            }

            return false;
        }

        public virtual void OnAfterPresetExport(VstHost host, IVstPlugin plugin)
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}