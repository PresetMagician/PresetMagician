using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.VendorPresetParser.Common;

namespace PresetMagician.VendorPresetParser.Parawave
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Parawave_Rapid : RecursiveFXPDirectoryParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1349997153};

        protected override string Extension { get; } = "fxp";

        public string Remarks { get; set; } =
            "Most audio previews are currently empty. Metadata is not being parsed at the moment.";

        protected override List<(string directory, PresetBank presetBank)> GetParseDirectories()
        {
            var dirs = new List<(string, PresetBank)>
            {
                (Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    @"Parawave\Rapid\Sound Presets"), GetRootBank()),
                (Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    @"Parawave\Rapid\Sound Presets"), GetRootBank())
            };

            return dirs;
        }

        protected override string GetParseDirectory()
        {
            throw new NotImplementedException();
        }
    }
}