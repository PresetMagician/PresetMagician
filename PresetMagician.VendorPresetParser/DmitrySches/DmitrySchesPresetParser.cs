using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using GSF.Collections;
using ICSharpCode.SharpZipLib.Zip.Compression;
using PresetMagician.Core.Models;

namespace Drachenkatze.PresetMagician.VendorPresetParser.DmitrySches
{
    public abstract class DmitrySchesPresetParser : RecursiveBankDirectoryParser
    {
        private byte[] _decodeBuffer;
        private const int DecodeBufferSize = 1024 * 1024 * 100;


        public override async Task DoScan()
        {
            _decodeBuffer = new byte[DecodeBufferSize];
            await base.DoScan();
            _decodeBuffer = null;
        }

        protected override byte[] ProcessFile(string fileName, PresetParserMetadata preset)
        {
            var inflater = new Inflater(false);
            inflater.SetInput(File.ReadAllBytes(fileName));
            var size = inflater.Inflate(_decodeBuffer);

            return _decodeBuffer.GetRange(0, size).ToArray();
        }
    }
}