using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.DmitrySches
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class DmitrySches_ThornCM : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1416122947};

        private static readonly string ParseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            @"Dmitry Sches\Thorn CM\Plug-In Presets");

        public override string BankLoadingNotes { get; set; } =
            $"Presets are loaded from {ParseDirectory}. First sub-folder defines the bank.";

        public override async Task DoScan()
        {
            var vc2parser = new DmitrySchesPresetParser(Plugin, "thorn", PresetDataStorer);
            await vc2parser.DoScan(RootBank, ParseDirectory);
        }
    }
}