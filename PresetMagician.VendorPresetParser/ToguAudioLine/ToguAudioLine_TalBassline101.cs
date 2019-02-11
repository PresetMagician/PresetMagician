using System;
using System.Collections.Generic;
using System.IO;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.ToguAudioLine
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class ToguAudioLine_TAlBassline101 : RecursiveVC2Parser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1648456497};

        protected override string Extension { get; } = "bassline";

        protected override string GetParseDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"ToguAudioLine\TAL-BassLine-101\presets");
        }
    }
}