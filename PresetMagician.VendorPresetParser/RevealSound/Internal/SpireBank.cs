using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using GSF.IO;
using Newtonsoft.Json;
using PresetMagician.Utils;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.RevealSound.Internal
{
     public class SpireBank
    {
        protected static readonly byte[] SBF_HEADER = {0x73, 0x62, 0x66};
        private static readonly byte[] SBF_DISK_HEADER = {0x00, 0x30, 0x39, 0x32, 0x30, 0x00};

        private const int BANK_NAME_LENGTH = 256;
        private const int BANK_NAME2_LENGTH = 256;
        private const int COMPANY_NAME_LENGTH = 256;
        private const int COMPANY_URL_LENGTH = 252;
        protected const int NUM_DISK_PRESETS = 128;

        public string BankName { get; private set; }
        public string BankName2 { get; private set; }
        public string CompanyName { get; private set; }
        public string CompanyUrl { get; private set; }

        public int NumParameters { get; private set; }
        public int SelectedProgramIndex { get; private set; }
        public List<SpirePreset> Presets { get; } = new List<SpirePreset>();

        public byte[] GenerateMemoryBank(SpirePreset preset, SpireJsonConfig config = null)
        {
            PrepareForExport(preset);
            
            using (var ms = new MemoryStream())
            {
                ms.Write(SBF_HEADER);
                ms.Write(preset.NumParameters);

                preset.Export(ms, true);
                ms.Write(SelectedProgramIndex);
                
                // Original bank data
                for (var i = 0; i < NUM_DISK_PRESETS; i++)
                {
                    Presets[i].Export(ms);
                }
                
                // Modified bank data
                for (var i = 0; i < NUM_DISK_PRESETS; i++)
                {
                    if (i == SelectedProgramIndex)
                    {
                        // Export the actual preset
                        preset.Export(ms);
                    }
                    else
                    {
                        Presets[i].Export(ms);
                    }
                }

                if (config != null)
                {
                    ms.WriteByte(0x00);
                    ms.WriteByte(0x7C);

                    var json = JsonConvert.SerializeObject(config);
                    var jsonBytes = Encoding.UTF8.GetBytes(json);
                    ms.Write(jsonBytes, 0, jsonBytes.Length);
                    ms.WriteByte(0x00);
                }
                return ms.ToArray();
            }
        }

        private void PrepareForExport(SpirePreset preset)
        {
            var fp = new SpirePreset();
            fp.ParseDiskFile(VendorResources.RevealSound_Spire_Blank);
            
            preset.FillMissingParametersFrom(fp);
            if (Presets.Count == 0)
            {
                // Generate blank bank
                for (var i = 0; i < NUM_DISK_PRESETS; i++)
                {
                    Presets.Add(preset.CreateBlank());
                }
            }

            if (Presets.Count != NUM_DISK_PRESETS)
            {
                throw new SpireException($"The number of presets is expected to be {NUM_DISK_PRESETS}, but " +
                                         $"the actual number is {Presets.Count}. This is either a bug in "+
                                         $"PresetMagician, a corrupt preset bank or an unknown format. Please report " +
                                         "this as a bug and include the affected file(s).");
            }

            var idx = FindPresetIndex(preset);

            if (idx != null)
            {
                SetSelectedProgramIndex((int) idx);
            }
            else
            {
                SetSelectedProgramIndex(0);
                Presets[0] = preset;
            }

            foreach (var p in Presets)
            {
                p.FillMissingParametersFrom(fp);
            }
        }
        
        public void SetSelectedProgramIndex(int index)
        {
            SelectedProgramIndex = index;
        }

        public int? FindPresetIndex(SpirePreset preset)
        {
            foreach (var p in Presets)
            {
                if (p.ProgramData.SequenceEqual(preset.ProgramData))
                {
                    return Presets.IndexOf(p);
                }
            }

            return null;
        }

        public void ParseDiskFile(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                ParseHeader(ms); // 3 bytes
                ParseDiskHeader(ms); // 6 bytes
                BankName = ms.ReadNullTerminatedString(BANK_NAME_LENGTH);
                BankName2 = ms.ReadNullTerminatedString(BANK_NAME2_LENGTH);
                CompanyName = ms.ReadNullTerminatedString(COMPANY_NAME_LENGTH);
                CompanyUrl = ms.ReadNullTerminatedString(COMPANY_URL_LENGTH);
                NumParameters = ParseNumParameters(ms);

                for (var i = 0; i < NUM_DISK_PRESETS; i++)
                {
                    var workaround = false;
                    var preset = new SpirePreset(NumParameters);

                    if (i + 1 == NUM_DISK_PRESETS)
                    {
                        workaround = true;
                    }

                    preset.ParseFromBank(ms, workaround);

                    Presets.Add(preset);
                }

                Debug.WriteLine($"Current offset: {ms.Position}, total length: {ms.Length}");
            }
        }


        private int ParseNumParameters(MemoryStream ms)
        {
            var paramLength = ms.ReadInt32();

            if (paramLength < 20 || paramLength > 8192)
            {
                throw new SpireException(
                    $"Expected the number of parameters between 20 and 8192, but got {paramLength} parameters. This seems wrong; aborting.");
            }

            return paramLength;
        }

        private void ParseDiskHeader(MemoryStream ms)
        {
            var headerBuffer = new byte[SBF_DISK_HEADER.Length];

            ms.Read(headerBuffer, 0, SBF_DISK_HEADER.Length);

            if (!headerBuffer.SequenceEqual(SBF_DISK_HEADER))
            {
                throw new SpireException(
                    $"Expected the disk header to be equal to {StringUtils.ByteArrayToHexString(SBF_DISK_HEADER)}, got {StringUtils.ByteArrayToHexString(headerBuffer)}");
            }
        }

        private void ParseHeader(MemoryStream ms)
        {
            var headerBuffer = new byte[SBF_HEADER.Length];

            ms.Read(headerBuffer, 0, SBF_HEADER.Length);

            if (!headerBuffer.SequenceEqual(SBF_HEADER))
            {
                throw new SpireException(
                    $"Expected the header to be equal to {StringUtils.ByteArrayToHexString(SBF_HEADER)}, got {StringUtils.ByteArrayToHexString(headerBuffer)}");
            }
        }
    }
}