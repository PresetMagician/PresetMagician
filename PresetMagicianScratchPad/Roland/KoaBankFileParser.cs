using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PresetMagicianScratchPad.Roland
{
    public class KoaBankFileParser
    {
        public string KoaFileHeader { get; set; }
        public int PresetNameLength { get; set; }
        public int PresetLength { get; set; }

        public KoaBankFileParser(string koaFileHeader, int presetNameLength, int presetLength)
        {
            if (presetLength == 0)
            {
                throw new Exception("PresetLength may not be 0");
            }

            if (presetNameLength == 0)
            {
                throw new Exception("PresetNameLength may not be 0");
            }

            if (koaFileHeader.Length == 0)
            {
                throw new Exception("KoaFileHeader not set");
            }
            PresetNameLength = presetNameLength;
            PresetLength = presetLength;
            KoaFileHeader = koaFileHeader;
        }

        public List<KoaPreset> Parse(string fileName)
        {
            var presets = new List<KoaPreset>();
            
            if (!File.Exists(fileName))
            {
                throw new IOException($"Trying to parse {fileName}, but the file does not exist.");
            }
            
            var data = File.ReadAllBytes(fileName);

            var fileHeader = Encoding.ASCII.GetBytes(KoaFileHeader);
            var fileHeaderBuffer = new byte[fileHeader.Length];
            var presetNameBuffer = new byte[PresetNameLength];
            
            var presetIndex = 0;
            using (var ms = new MemoryStream(data))
            {
                ms.Read(fileHeaderBuffer, 0, fileHeader.Length);

                if (!fileHeaderBuffer.SequenceEqual(fileHeader))
                {
                    throw new Exception($"Unable to parse {fileName}: The file header {Encoding.ASCII.GetString(fileHeaderBuffer)} does not match the expected header {KoaFileHeader}");
                }

                while (ms.Position < ms.Length)
                {
                    var presetBuffer = new byte[PresetLength];
                    ms.Read(presetNameBuffer, 0, PresetNameLength);
                    ms.Read(presetBuffer, 0, PresetLength);
                    var preset = new KoaPreset();

                    preset.BankFile = fileName;
                    preset.Index = presetIndex;
                    preset.PresetName = Encoding.ASCII.GetString(presetNameBuffer);
                    preset.PresetData = presetBuffer;
                    
                    presets.Add(preset);
                    presetIndex++;
                }
            }

            return presets;


        }
    }
}