using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AudioThing_TypeA : AudioThing, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1954107457};

        protected override List<(string directory, PresetBank presetBank)> GetParseDirectories()
        {
            return new List<(string, PresetBank)>
            {
                (Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    @"AudioThing\Presets\TypeA"), GetRootBank()),
                (Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                    @"AudioThing\Presets\TypeA"), GetRootBank())
            };
        }

        protected override string GetParseDirectory()
        {
            throw new NotImplementedException();
        }
    }
}