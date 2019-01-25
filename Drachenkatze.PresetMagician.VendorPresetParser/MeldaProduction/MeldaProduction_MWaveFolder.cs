using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MWaveFolder : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1297572166};

        protected override string PresetFile { get; } = "MWaveFolderpresets.xml";

        protected override string RootTag { get; } = "MWaveFolderpresets";
    }
}