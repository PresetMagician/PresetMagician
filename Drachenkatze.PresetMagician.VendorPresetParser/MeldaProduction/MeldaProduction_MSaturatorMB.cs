using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MSaturatorMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296908876};

        protected override string PresetFile { get; } =
            "MMultiBandSaturatorpresets.xml";

        protected override string RootTag { get; } = "MMultiBandSaturatorpresetspresets";
    }
}