using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace Drachenkatze.PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MChannelMatrix : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296263245};

        protected override string PresetFile { get; } = "MChannelMatrixpresets.xml";

        protected override string RootTag { get; } = "MChannelMatrixpresets";
    }
}