using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.VendorPresetParser.Common;

namespace PresetMagician.VendorPresetParser.InitialAudio
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class InitialAudio_HeatUp3 : RecursiveVC2Parser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1214608435};

        protected override string Extension { get; } = "heatUpPreset";

        public override string Remarks { get; set; } =
            "This is a beta of the HeatUp3 preset parser and might not work correctly.";

        public override void Init()
        {
            BankLoadingNotes = $"Presets are loaded from {GetParseDirectory()}";
            base.Init();
        }

        protected override string GetParseDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"IgniteVST\HeatUp3\Presets");
        }
    }
}