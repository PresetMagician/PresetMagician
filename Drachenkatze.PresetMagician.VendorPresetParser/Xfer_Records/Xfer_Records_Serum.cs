using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.Utils;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Xfer_Records
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Xfer_Records_Serum : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1483109208};

       protected string GetDataDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Xfer\Serum Presets\Presets");
          
        }
       
       public override async Task DoScan()
       {
           var parser = new RecursiveFXPDirectoryParser(Plugin, "fxp", PresetDataStorer);
           await parser.DoScan(RootBank, GetDataDirectory());
       }
    }
}