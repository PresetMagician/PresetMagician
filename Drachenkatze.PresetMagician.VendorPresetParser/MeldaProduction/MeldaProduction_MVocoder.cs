using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MVocoder : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297510243};

        protected override string PresetFile { get; } = "MVocoderpresets.xml";

        protected override string RootTag { get; } = "MVocoderpresetspresets";
    }
}