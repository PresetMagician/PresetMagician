using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AirTech_AIRDistortion : AirTech, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1953711169};
        protected override string Extension { get; } = "tfx";

  

        protected override string GetParseDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                @"AIR Music Technology\AIR Distortion\Presets");
        }

        protected override Tfx.Tfx GetTfxParser()
        {
            return new TfxAIRDistortion();
        }
    }
}