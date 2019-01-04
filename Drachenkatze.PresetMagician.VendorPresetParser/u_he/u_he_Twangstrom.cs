using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_Twangstrom : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1969771607 };

        public void ScanBanks()
        {
            H2PScanBanks("Twangstrom.data", "Twangstrom", false);
            H2PScanBanks("Twangstrom.data", "Twangstrom", true);
        }
    }
}