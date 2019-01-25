using System.Collections.Generic;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MDrummer : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296323149, 1296323126, 1296323121};

        protected override string PresetFile { get; } = "MDrummerpresets.xml";

        protected override string RootTag { get; } = "MDrummerpresets";
    }
}