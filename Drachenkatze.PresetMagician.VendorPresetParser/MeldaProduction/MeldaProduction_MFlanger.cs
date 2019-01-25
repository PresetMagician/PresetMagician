using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MFlanger : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296123174};

        protected override string PresetFile { get; } = "MFlangerpresets.xml";

        protected override string RootTag { get; } = "MFlangerpresetspresets";
    }
}