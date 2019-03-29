using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GSF.Collections;
using ICSharpCode.SharpZipLib.Zip.Compression;
using PresetMagician.Core.Models;
using PresetMagician.VendorPresetParser.Common;

namespace PresetMagician.VendorPresetParser.DmitrySches
{
    public abstract class DmitrySchesPresetParser : RecursiveBankDirectoryParser
    {
        private const int DecodeBufferSize = 1024 * 1024 * 100;
        private byte[] _decodeBuffer;


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