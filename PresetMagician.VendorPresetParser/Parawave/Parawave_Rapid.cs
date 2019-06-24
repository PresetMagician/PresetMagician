using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSF.Collections;
using GSF.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;
using JetBrains.Annotations;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.VendorPresetParser.Common;
using Type = PresetMagician.Core.Models.Type;

namespace PresetMagician.VendorPresetParser.Parawave
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Parawave_Rapid : RecursiveFXPDirectoryParser, IVendorPresetParser
    {
        private const int DecodeBufferSize = 1024 * 1024 * 100;
        private byte[] _decodeBuffer;

        public override List<int> SupportedPlugins => new List<int> {1349997153};

        protected override string Extension { get; } = "fxp";

        public override async Task DoScan()
        {
            _decodeBuffer = new byte[DecodeBufferSize];
            await base.DoScan();
            _decodeBuffer = null;
        }

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

        protected override byte[] ProcessFile(string fileName, PresetParserMetadata preset)
        {
            var chunkData = base.ProcessFile(fileName, preset);

            var ms = new MemoryStream(chunkData);

            var parameterSize = ms.ReadInt32();
            ms.Seek(parameterSize, SeekOrigin.Current);
            var metadataSize = ms.ReadInt32();

            var metadataBuf = new byte[metadataSize];
            ms.Read(metadataBuf, 0, metadataSize);

            var inflater = new Inflater(false);
            inflater.SetInput(metadataBuf);
            var size = inflater.Inflate(_decodeBuffer);

            var metadata = _decodeBuffer.GetRange(0, size).ToArray();

            var metadataString = Encoding.UTF8.GetString(metadata);

            var tokens = metadataString.Split(',');

            var metadataDictionary = new Dictionary<string, string>();

            var isKey = true;
            var key = "";
            foreach (var token in tokens)
            {
                if (isKey)
                {
                    key = token;
                }
                else
                {
                    try
                    {
                        metadataDictionary.Add(key, token);
                    }
                    catch (ArgumentException)
                    {
                        // Do nothing
                    }
                }

                isKey = !isKey;
            }

            if (metadataDictionary.ContainsKey("Comment"))
            {
                preset.Comment = metadataDictionary["Comment"];
            }

            if (metadataDictionary.ContainsKey("Author"))
            {
                preset.Author = metadataDictionary["Author"];
            }

            ApplyType(int.Parse(metadataDictionary["Type"]), preset);

            return chunkData;
        }

        private void ApplyType(int id, PresetParserMetadata preset)
        {
            switch (id)
            {
                case 1: break;
                case 2: // Arpeggiated
                    preset.Characteristics.Add(new Characteristic {CharacteristicName = "Arpeggiated"});
                    break;
                case 3: //Bass
                    preset.Types.Add(new Type {TypeName = "Bass"});
                    break;
                case 4: // Chord
                    preset.Characteristics.Add(new Characteristic {CharacteristicName = "Chord"});
                    break;
                case 5: // Effect
                    preset.Types.Add(new Type {TypeName = "Sound Effects"});
                    break;

                case 6: // Drumkit
                    preset.Types.Add(new Type {TypeName = "Drums", SubTypeName = "Kit"});
                    break;
                case 7: // Drumloop
                    preset.Types.Add(new Type {TypeName = "Drums"});
                    preset.Characteristics.Add(new Characteristic {CharacteristicName = "Sequence / Loop"});
                    break;
                case 8: // Gated
                    preset.Types.Add(new Type {TypeName = "Arp / Sequence", SubTypeName = "Gated"});
                    break;
                case 9: // Lead
                    preset.Characteristics.Add(new Characteristic {CharacteristicName = "Lead"});
                    break;
                case 10: // Melody
                    preset.Characteristics.Add(new Characteristic {CharacteristicName = "Melodic"});
                    break;
                case 11: // Pad
                    preset.Types.Add(new Type {TypeName = "Synth Pad"});
                    break;
                case 12: // Pluck
                    preset.Types.Add(new Type {TypeName = "Synth Pluck"});
                    break;
                case 13: // Sequence
                    preset.Types.Add(new Type {TypeName = "Arp / Sequence"});
                    break;
                case 14: // Splitted
                    preset.Types.Add(new Type {TypeName = "Combination"});
                    break;
                case 15: // Synth
                    preset.Types.Add(new Type {TypeName = "Synth Misc"});
                    break;
                case 16: // Texture
                    preset.Characteristics.Add(new Characteristic {CharacteristicName = "Textural"});
                    break;
                case 17: // Track
                    preset.Types.Add(new Type {TypeName = "Arp / Sequence"});
                    break;
                case 18: // Bell
                    preset.Types.Add(new Type {TypeName = "Percussion", SubTypeName = "Bell"});
                    break;
                case 19: // Brass
                    preset.Types.Add(new Type {TypeName = "Brass"});
                    break;
                case 20: // Drum
                    preset.Types.Add(new Type {TypeName = "Drums"});
                    break;
                case 21: // Guitar
                    preset.Types.Add(new Type {TypeName = "Guitar"});
                    break;
                case 22: // Mallet
                    preset.Types.Add(new Type {TypeName = "Mallet Instruments"});
                    break;
                case 23: // Organ
                    preset.Types.Add(new Type {TypeName = "Organ"});
                    break;
                case 24: // Piano
                    preset.Types.Add(new Type {TypeName = "Piano / Keys"});
                    break;
                case 25: // String
                    preset.Types.Add(new Type {TypeName = "Bowed Instruments", SubTypeName = "Synth"});
                    break;
                case 26: // Vocal
                    preset.Types.Add(new Type {TypeName = "Vocal"});
                    break;
                case 27: // Woodwind
                    preset.Types.Add(new Type {TypeName = "Mallet Instruments", SubTypeName = "Wood"});
                    break;
                case 28: // Keys
                    preset.Types.Add(new Type {TypeName = "Piano / Keys"});
                    break;
                case 29: // Solo
                    preset.Types.Add(new Type {TypeName = "Solo"});
                    break;
                default:
                    Logger.Error(
                        $"Unknown type id {id} for preset {preset.PresetName}. Seems like there were additional " +
                        "types introduced to Rapid since this preset parser was developed. " +
                        "Please report this as a bug.");
                    break;
            }
        }

        protected override string GetParseDirectory()
        {
            throw new NotImplementedException();
        }
    }
}