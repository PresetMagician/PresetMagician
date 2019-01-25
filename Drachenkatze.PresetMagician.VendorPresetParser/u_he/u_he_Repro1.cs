using System.Collections.Generic;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.u_he
{
    internal class u_he_Repro1 : u_he, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1968332593};

        public void ScanBanks()
        {
            H2PScanBanks("Repro-1.data", "Repro-1", false);
            H2PScanBanks("Repro-1.data", "Repro-1", true);
        }
    }
}