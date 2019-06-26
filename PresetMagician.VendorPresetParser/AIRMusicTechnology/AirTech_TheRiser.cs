using System;
using System.Collections.Generic;
using Catel.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AirTech_TheRiser : AirTech, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1920160372};
        protected override string Extension { get; } = "tfx";

        protected override string GetParseDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                @"AIR Music Technology\theRiser\Presets");
        }

        protected override Tfx.Tfx GetTfxParser()
        {
            return new TfxTheRiser();
        }

        public override void OnPluginLoad()
        {
            PluginInstance.DisableTimeInfo();
        }

        public override void OnPluginUnload()
        {
            PluginInstance.EnableTimeInfo();
        }
    }
}