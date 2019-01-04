using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    internal class u_he_Zebrify : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1397578034 };

        public void ScanBanks()
        {
            H2PScanBanks("Zebra2.data", "Zebrify", false);
            H2PScanBanks("Zebra2.data", "Zebrify", true);
        }
    }
}