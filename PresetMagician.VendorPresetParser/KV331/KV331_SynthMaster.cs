using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.VendorPresetParser.Common;

namespace PresetMagician.VendorPresetParser.KV331
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class KV331_SynthMaster : RecursiveBankDirectoryParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1399665257};

        protected override string Extension { get; } = "smpr";

        public override string Remarks { get; set; } = "Metadata is not being parsed at the moment.";

        protected override List<(string directory, PresetBank presetBank)> GetParseDirectories()
        {
            var dirs = new List<(string, PresetBank)>
            {
                (Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    @"KV331 Audio\SynthMaster\Presets\"), GetRootBank()),
                (Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    @"SynthMaster\Presets"), GetRootBank())
            };

            return dirs;
        }

        protected override string GetParseDirectory()
        {
            throw new NotImplementedException();
        }
    }
}