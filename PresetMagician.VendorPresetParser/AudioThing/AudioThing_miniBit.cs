using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using Syroot.Windows.IO;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AudioThing_miniBit : AudioThing, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1833525876};


        protected override string GetParseDirectory()
        {
            return Path.Combine(new KnownFolder(KnownFolderType.PublicDocuments).Path,
                @"AudioThing\Presets\miniBit\Factory");
        }
    }
}