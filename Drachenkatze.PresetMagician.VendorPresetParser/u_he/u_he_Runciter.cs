using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_Runciter : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1382232375 };

        public void ScanBanks()
        {
            H2PScanBanks("Uhbik.data", "Runciter", false);
            H2PScanBanks("Uhbik.data", "Runciter", true);
        }
    }
}