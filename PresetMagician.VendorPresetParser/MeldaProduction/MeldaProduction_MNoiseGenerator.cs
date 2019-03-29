using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MNoiseGenerator : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296985961};

        protected override string PresetFile { get; } = "MNoiseGeneratorpresets.xml";

        protected override string RootTag { get; } = "MNoiseGeneratorpresetspresets";
    }
}