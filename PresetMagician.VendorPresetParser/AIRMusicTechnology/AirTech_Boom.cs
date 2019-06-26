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
    public class AirTech_Boom : AirTech, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1836019522};
        protected override string Extension { get; } = "tfx";

        protected override string GetParseDirectory()
        {
            try
            {
                return Path.Combine(GetContentRegistryValue(@"Software\AirMusicTech\Boom", "Content"), "Presets");
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected override Tfx.Tfx GetTfxParser()
        {
            return new TfxBoom();
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