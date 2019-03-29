using System;
using System.Collections.Generic;
using Catel.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.AIRMusicTechnology;
using PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AIRMusicTechnology
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AirTech_Hybrid3: AirTech, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1685217864};
        protected override string Extension { get; } = "tfx";
        
        public override string Remarks { get; set; } =
            "Audio Previews are non-functional for this plugin";
        
        protected override string GetParseDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                @"AIR Music Technology\Hybrid\Presets");
        }

        protected override Tfx GetTfxParser()
        {
            return new TfxHybrid3();
        }

    }
}