using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MFlangerMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296908870};

        protected override string PresetFile { get; } =
            "MMultiBandFlangerpresets.xml";

        protected override string RootTag { get; } = "MMultiBandFlangerpresetspresets";
    }
}