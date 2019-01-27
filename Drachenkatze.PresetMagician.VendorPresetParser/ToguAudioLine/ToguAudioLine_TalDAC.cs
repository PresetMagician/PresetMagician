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
    public class ToguAudioLine_TalDAC : RecursiveVC2Parser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1684104024};

        protected override string Extension { get; } = "taldac";

        protected override string GetParseDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"ToguAudioLine\TAL-Dac\presets");
            ;
        }

        protected override PresetBank GetRootBank()
        {
            return RootBank.CreateRecursive(BankNameFactory);
        }
    }
}