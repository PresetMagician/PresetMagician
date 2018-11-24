using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    internal class u_he_ACE : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1633895765 };

        public void ScanBanks()
        {
            H2PScanBanks("ACE.data", "ACE", false);
            H2PScanBanks("ACE.data", "ACE", true);
        }
    }
}