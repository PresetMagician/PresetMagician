using System.Collections.Generic;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_MFM2 : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296452914};

        public void ScanBanks()
        {
            H2PScanBanks("MFM2.data", "MFM2", false);
            H2PScanBanks("MFM2.data", "MFM2", true);
        }
    }
}