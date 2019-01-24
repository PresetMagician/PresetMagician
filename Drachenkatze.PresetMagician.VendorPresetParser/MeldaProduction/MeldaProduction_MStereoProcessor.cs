using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MStereoProcessor : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296114724};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MStereoProcessorpresets.xml", "MStereoProcessorpresetspresets");
        }
    }
}