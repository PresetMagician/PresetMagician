using System;
using System.Collections.Generic;
using Catel.IO;
using Drachenkatze.PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AIRMusicTechnology
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class AirTech_Xpand2: RecursiveBankDirectoryParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {845443160};
        protected override string Extension { get; } = "tfx";
        
        protected override string GetParseDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                @"AIR Music Technology\Xpand!2\Presets");
        }
        
        protected override byte[] ProcessFile(string fileName, PresetParserMetadata preset)
        {
            var tfx = new TfxXpand2();
            var patchDirectory = GetParseDirectory();
            tfx.Parse(patchDirectory, fileName.Replace(patchDirectory + "\"", ""));


            return tfx.GetDataToWrite();
        }
    }
}