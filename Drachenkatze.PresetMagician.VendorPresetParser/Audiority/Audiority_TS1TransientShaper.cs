using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Audiority
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Audiority_TS1TransientShaper : Audiority, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1332836457};

        protected override string GetDataDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Audiority\Presets\TS-1 Transient Shaper");
        }
        
        public override async Task DoScan()
        {
            var vc2parser = new VC2Parser(Plugin, "aup", PresetDataStorer);
            await vc2parser.DoScan(RootBank.CreateRecursive(BankNameFactory), GetDataDirectory());
        }
    }
}