using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MModernCompressor : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296920387};

        protected override string PresetFile { get; } =
            "MModernCompressorpresets.xml";

        protected override string RootTag { get; } = "MModernCompressorpresetspresets";
    }
}