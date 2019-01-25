using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MFreqShifter : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296131398};

        protected override string PresetFile { get; } = "MFreqShifterpresets.xml";

        protected override string RootTag { get; } = "MFreqShifterpresetspresets";
    }
}