using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public class NullPresetParser : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override bool IsNullParser => true;

        public void ScanBanks()
        {
        }

        public override bool CanHandle()
        {
            return true;
        }
    }
}