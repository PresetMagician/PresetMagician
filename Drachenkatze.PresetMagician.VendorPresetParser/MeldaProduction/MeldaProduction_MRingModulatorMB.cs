using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MRingModulatorMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1299018349};

        protected override string PresetFile { get; } =
            "MMultiBandRingModulatorpresets.xml";

        protected override string RootTag { get; } = "MMultiBandRingModulatorpresetspresets";
    }
}