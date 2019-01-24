using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.DmitrySches
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class DmitrySches_Thorn: AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1416122990};

        private static readonly string ParseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            @"Dmitry Sches\Thorn\Plug-In Presets");
        
        public override string BankLoadingNotes { get; set; } = $"Presets are loaded from {ParseDirectory}. First sub-folder defines the bank.";

        public void ScanBanks()
        {
            DoScan(RootBank, ParseDirectory);
        }
        
        protected void DoScan(PresetBank rootBank, string directory)
        {
            var vc2parser = new DmitrySchesPresetParser(Plugin, "thorn",Presets);
            vc2parser.DoScan(rootBank, directory);
        }
    }
}