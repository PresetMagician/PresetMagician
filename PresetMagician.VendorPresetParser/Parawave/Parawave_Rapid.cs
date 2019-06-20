using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public string Remarks { get; set; } =
            "Most audio previews are currently empty. Metadata is not being parsed at the moment.";

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
                    catch (ArgumentException e)
                    {
                        Debug.WriteLine($"{key} already exists");
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

            foreach (var d in metadataDictionary)
            {
                Debug.WriteLine($"{d.Key}: {d.Value}");
            }

            return chunkData;
        }

        protected override string GetParseDirectory()
        {
            throw new NotImplementedException();
        }
    }
}