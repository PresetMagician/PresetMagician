using System.Collections.Generic;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    internal class u_he_Zebralette : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1397572659};

        public void ScanBanks()
        {
            H2PScanBanks("Zebra2.data", "Zebralette", false);
            H2PScanBanks("Zebra2.data", "Zebralette", true);
        }
    }
}