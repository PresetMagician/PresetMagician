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
    public class AirTech_Velvet : AirTech, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1953918038};
        protected override string Extension { get; } = "tfx";

        protected override string GetParseDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                @"AIR Music Technology\Velvet\Presets");
        }

        protected override Tfx.Tfx GetTfxParser()
        {
            return new TfxVelvet();
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