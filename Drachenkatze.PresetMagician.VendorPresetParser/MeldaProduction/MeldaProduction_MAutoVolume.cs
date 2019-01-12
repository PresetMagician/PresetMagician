using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MAutoVolume: MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296135542};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MAutoVolumepresets.xml", "MAutoVolumepresetspresets");
        }
    }
}