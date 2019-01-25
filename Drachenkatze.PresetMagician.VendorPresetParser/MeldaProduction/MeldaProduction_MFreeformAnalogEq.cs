using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MFreeformAnalogEq : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296449893};

        protected override string PresetFile { get; } =
            "MFreeformAnalogEqpresets.xml";

        protected override string RootTag { get; } = "MFreeformAnalogEqpresetspresets";
    }
}