using System;
using System.Collections.Generic;
using Catel.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.VendorPresetParser.AIRMusicTechnology;
using PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AIRMusicTechnology
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AirTech_Loom: AirTech, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1836019532};
        protected override string Extension { get; } = "tfx";
        
        public override string Remarks { get; set; } =
            "Audio Previews are non-functional for this plugin";
        
       
        protected override List<(string directory, PresetBank presetBank)> GetParseDirectories()
        {
            return new List<(string, PresetBank)>
            {
                (Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    @"AIR Music Technology\Loom\Presets"), GetRootBank()),
                (Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    @"AIR Music Technology\Loom\Presets"), GetRootBank())
            };
        }

        protected override string GetParseDirectory()
        {
            throw new NotImplementedException();
        }

        protected override Tfx GetTfxParser()
        {
            return new TfxLoom();
        }
    }
}