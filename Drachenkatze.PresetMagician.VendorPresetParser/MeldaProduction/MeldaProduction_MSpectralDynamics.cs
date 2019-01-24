using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MSpectralDynamics : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296114745, 1296114801};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MSpectralDynamicspresets.xml", "MSpectralDynamicspresetspresets");
        }
    }
}