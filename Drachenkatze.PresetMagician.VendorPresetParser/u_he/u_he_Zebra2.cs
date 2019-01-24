using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    internal class u_he_Zebra2 : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1397572658};

        public void ScanBanks()
        {
            H2PScanBanks("Zebra2.data", "Zebra2", false);
            H2PScanBanks("Zebra2.data", "Zebra2", true);
        }
    }
}