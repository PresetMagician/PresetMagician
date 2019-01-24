using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.ToguAudioLine
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class ToguAudioLine_TalDAC : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1684104024};

        private static readonly string ParseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"ToguAudioLine\TAL-Dac\presets");

        public override string BankLoadingNotes { get; set; } =
            $"Presets are loaded from {ParseDirectory}. First sub-folder defines the bank.";

        public override async Task DoScan()
        {
            var parser = new VC2Parser(Plugin, "taldac", PresetDataStorer);
            await parser.DoScan(RootBank.CreateRecursive(BankNameFactory), ParseDirectory);
        }
    }
}