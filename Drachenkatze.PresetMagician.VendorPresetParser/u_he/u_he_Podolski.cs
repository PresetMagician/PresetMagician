using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    internal class u_he_Podolski : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1349477487 };

        public void ScanBanks()
        {
            H2PScanBanks("Podolski.data", "Podolski", false);
        }
    }
}