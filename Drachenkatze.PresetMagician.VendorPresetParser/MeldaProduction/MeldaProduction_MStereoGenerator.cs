using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MStereoGenerator : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297314887};

        protected override string PresetFile { get; } =
            "MStereoGeneratorpresets.xml";

        protected override string RootTag { get; } = "MStereoGeneratorpresetspresets";
    }
}