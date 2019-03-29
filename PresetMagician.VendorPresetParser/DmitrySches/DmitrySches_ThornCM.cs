using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser.DmitrySches
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class DmitrySches_ThornCM : DmitrySchesPresetParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1416122947};

        protected override string Extension { get; } = "thorn";

        protected override string GetParseDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Dmitry Sches\Thorn CM\Plug-In Presets");
        }
    }
}