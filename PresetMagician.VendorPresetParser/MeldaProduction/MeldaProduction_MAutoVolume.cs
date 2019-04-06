using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.MeldaProduction
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class MeldaProduction_MAutoVolume : MeldaProduction, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296135542};

        protected override string PresetFile { get; } = "MAutoVolumepresets.xml";

        protected override string RootTag { get; } = "MAutoVolumepresetspresets";
    }
}