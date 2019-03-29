using System;
using System.Collections.Generic;
using Catel.IO;
using Drachenkatze.PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AIRMusicTechnology
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AirTech_Boom: AirTech, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1836019522};
        protected override string Extension { get; } = "tfx";
        
        public override string Remarks { get; set; } =
            "Audio Previews are non-functional for this plugin";
        
        protected override string GetParseDirectory()
        {
            return Path.Combine(GetContentRegistryValue(@"Software\\AirMusicTech\Boom", "Content"), "Presets");
           
        }

        protected override Tfx.Tfx GetTfxParser()
        {
            return new TfxBoom();
        }
    }
}