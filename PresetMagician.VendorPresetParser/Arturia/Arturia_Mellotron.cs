using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.Arturia
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Arturia_Mellotron : Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1296847950};

        public override int AudioPreviewPreDelay { get; set; } = 400;

        public override string Remarks { get; set; } = "This plugin takes a variable amount of time to load a preset. " +
                                              Environment.NewLine +
                                              "If an audio preview is missing, increase the audio preview pre-delay "+
                                              "in the plugin settings and re-export the affected presets.";

        protected override List<string> GetInstrumentNames()
        {
            return new List<string> {"Mellotron"};
        }
    }
}