using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MStereoProcessor : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296114724};

        protected override string PresetFile { get; } =
            "MStereoProcessorpresets.xml";

        protected override string RootTag { get; } = "MStereoProcessorpresetspresets";
    }
}