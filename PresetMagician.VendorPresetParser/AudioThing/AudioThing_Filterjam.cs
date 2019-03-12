using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AudioThing_Filterjam : AudioThing, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1179279725};


        protected override string GetParseDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                @"AudioThing\Presets\Filterjam");
        }
    }
}