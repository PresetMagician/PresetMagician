using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;

namespace Drachenkatze.PresetMagician.VendorPresetParser.TUBerln
{
    public class Synister : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1298748272};

        private static string GetDataDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                @"Synister");
        }
       
       public override async Task DoScan()
       {
           var parser = new VC2Parser(Plugin, "xml", PresetDataStorer);
           await parser.DoScan(RootBank.CreateRecursive(BankNameFactory), GetDataDirectory());
       }
    }
}