using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MAutoPitch : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1298232660};

        protected override string PresetFile { get; } = "MAutoPitchpresets.xml";

        protected override string RootTag { get; } = "MAutoPitchpresetspresets";
    }
}