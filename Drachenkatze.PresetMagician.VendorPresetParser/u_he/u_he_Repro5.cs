using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    internal class u_he_Repro5 : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1968332597};

        public void ScanBanks()
        {
            H2PScanBanks("Repro-1.data", "Repro-5", false);
            H2PScanBanks("Repro-1.data", "Repro-5", true);
        }
    }
}