using System.Collections.Generic;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    public class u_he_ColourCopy : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1967946098};

        public void ScanBanks()
        {
            H2PScanBanks("ColourCopy.data", "ColourCopy", false);
            H2PScanBanks("ColourCopy.data", "ColourCopy", true);
        }
    }
}