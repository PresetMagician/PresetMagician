using System.Collections.Generic;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MWaveFolderMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1299011430};

        protected override string PresetFile { get; } =
            "MMultiBandWaveFolderpresets.xml";

        protected override string RootTag { get; } = "MMultiBandWaveFolderpresets";
    }
}