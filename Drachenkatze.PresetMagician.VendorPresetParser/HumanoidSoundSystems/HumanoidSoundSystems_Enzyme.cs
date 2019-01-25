using System;
using System.Collections.Generic;
using System.IO;
using Drachenkatze.PresetMagician.Utils;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using Drachenkatze.PresetMagician.VSTHost.VST;
using GSF;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.HumanoidSoundSystems
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class HumanoidSoundSystems_Enzyme : RecursiveBankDirectoryParser, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1162762841};

        protected override string Extension { get; } = "enz";

        protected override string GetParseDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Humanoid Sound Systems\Enzyme\EnzymeData\Presets");
        }

        protected override byte[] ProcessFile(string fileName, Preset preset)
        {
            var data = File.ReadAllBytes(fileName);
            var ms = new MemoryStream();
            ms.Write(data, 0, data.Length);

            var relativeFile = BinaryFile.StringToByteArray(fileName.Replace(GetParseDirectory(), ""));
            var tuningFile = BinaryFile.StringToByteArray(@"Tunings\Basic\Default.tun");

            ms.Seek(0, SeekOrigin.End);
            ms.WriteByte(0x01);
            ms.WriteByte((byte) relativeFile.Length);
            ms.Write(relativeFile, 0, relativeFile.Length);

            ms.WriteByte(0x01);
            ms.WriteByte((byte) tuningFile.Length);
            ms.Write(tuningFile, 0, tuningFile.Length);

            ms.Seek(0, SeekOrigin.Begin);

            var ms2 = new MemoryStream();
            ms2.Write(LittleEndian.GetBytes(ms.Length), 0, 4);
            ms.WriteTo(ms2);
            ms2.Seek(0, SeekOrigin.Begin);

            return ms2.ToArray();
        }

    }
}