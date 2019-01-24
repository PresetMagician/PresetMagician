using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    internal class u_he_TyrellN6 : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1952017974};

        public void ScanBanks()
        {
            H2PScanBanks("TyrellN6.data", "TyrellN6", false);
            H2PScanBanks("TyrellN6.data", "TyrellN6", true);
        }
    }
}