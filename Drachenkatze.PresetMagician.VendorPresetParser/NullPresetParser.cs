using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public class NullPresetParser : IVendorPresetParser
    {
        public List<PresetBank> Banks => new List<PresetBank>();

        public IVstPlugin VstPlugin { get; set; }

        public virtual string Remarks { get; set; }

        public string PresetParserType => GetType().Name.ToString();

        public bool IsNullParser => true;

        public int NumPresets => 0;

        public void ScanBanks()
        {
        }

        public virtual void OnAfterPresetExport(VstHost host, IVstPlugin plugin)
        {

        }

        public bool CanHandle()
        {
            return true;
        }
    }
}