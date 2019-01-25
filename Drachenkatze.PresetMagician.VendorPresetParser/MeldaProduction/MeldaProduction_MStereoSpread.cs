using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MStereoSpread : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297314899};

        protected override string PresetFile { get; } = "MStereoSpreadpresets.xml";

        protected override string RootTag { get; } = "MStereoSpreadpresetspresets";
    }
}