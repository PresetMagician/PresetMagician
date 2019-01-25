using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AudioThing_Megaphone : AudioThing, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1298485352};

        protected override PresetBank GetRootBank()
        {
            return RootBank.CreateRecursive(BankNameFactory);
        }
        
        protected override string GetParseDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"AudioThing\Presets\Megaphone");
        }
    }
}