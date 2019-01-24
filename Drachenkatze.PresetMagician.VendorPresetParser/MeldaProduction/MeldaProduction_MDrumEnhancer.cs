using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MDrumEnhancer : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296319854};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MDrumEnhancerpresets.xml", "MDrumEnhancerpresets");
        }
    }
}