using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MTremoloMB : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1299018861};

        protected override string PresetFile { get; } = "MMultiBandTremolopresets.xml";

        protected override string RootTag { get; } = "MMultiBandAutopanpresets";
    }
}