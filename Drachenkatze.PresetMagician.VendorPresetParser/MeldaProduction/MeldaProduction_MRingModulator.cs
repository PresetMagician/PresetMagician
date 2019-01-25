using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MRingModulator : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296131382};

        protected override string PresetFile { get; } = "MRingModulatorpresets.xml";

        protected override string RootTag { get; } = "MRingModulatorpresets";
    }
}