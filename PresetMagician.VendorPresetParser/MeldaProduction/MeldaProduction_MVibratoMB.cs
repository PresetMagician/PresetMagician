using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MVibratoMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1299011177};

        protected override string PresetFile { get; } =
            "MMultiBandVibratopresets.xml";

        protected override string RootTag { get; } = "MMultiBandVibratopresetspresets";
    }
}