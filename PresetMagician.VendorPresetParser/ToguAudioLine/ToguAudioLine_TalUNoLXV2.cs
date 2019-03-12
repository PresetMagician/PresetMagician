using System;
using System.Collections.Generic;
using System.IO;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace Drachenkatze.PresetMagician.VendorPresetParser.ToguAudioLine
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class ToguAudioLine_TalUNoLXV2 : RecursiveVC2Parser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1970171700};

        protected override string Extension { get; } = "pjunoxl";

        protected override string GetParseDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"ToguAudioLine\TAL-U-No-LX\presets");
        }
    }
}