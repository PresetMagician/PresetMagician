using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    [UsedImplicitly]
    public class VoidPresetParser : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override string Remarks { get; set; } =
            "Plugin doesn't seem to have preset loading/saving capabilities";

        public override List<int> SupportedPlugins => new List<int>
        {
            1951355500, 
            1919243824, 
            1364228170,
            1098206310, // Audiority Low Filter
            1098208102, // Audiority Side Filter
            0
        };

        public void ScanBanks()
        {
        }
    }
}