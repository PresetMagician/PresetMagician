using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_Satin : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1969771348};

        public void ScanBanks()
        {
            H2PScanBanks("Satin.data", "Satin", false);
            H2PScanBanks("Satin.data", "Satin", true);
        }
    }
}