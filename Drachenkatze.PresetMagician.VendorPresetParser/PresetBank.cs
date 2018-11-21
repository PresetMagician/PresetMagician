using Drachenkatze.PresetMagician.VSTHost.VST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public class PresetBank
    {
        public List<VSTPreset> VSTPresets;
        public String BankName;

        public PresetBank ()
        {
            VSTPresets = new List<VSTPreset>();
        }
    }
}
