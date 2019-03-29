using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.Arturia
{
    [UsedImplicitly]
    public class Arturia_AnalogLab3 : Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1097621810};

        public override string Remarks { get; set; } =
            "Only AnalogLab3 multi presets are currently supported, as the format has not been fully reverse engineered. Audio previews are not working at the moment.";

        protected override List<string> GetInstrumentNames()
        {
            return new List<string> {"Analog Lab"};
        }
    }
}