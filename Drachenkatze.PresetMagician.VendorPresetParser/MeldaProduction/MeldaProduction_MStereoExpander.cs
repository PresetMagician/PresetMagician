using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MStereoExpander : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296131425};

        public void ScanBanks()
        {
            ScanPresetXMLFile("MStereoExpanderpresets.xml", "MStereoExpanderpresets");
        }
    }
}