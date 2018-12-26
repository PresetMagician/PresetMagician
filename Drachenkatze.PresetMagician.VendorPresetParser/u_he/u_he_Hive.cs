using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    internal class u_he_Hive : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1749636677 };

        public void ScanBanks()
        {
            H2PScanBanks("Hive.data", "Hive", false);
        }
    }
}