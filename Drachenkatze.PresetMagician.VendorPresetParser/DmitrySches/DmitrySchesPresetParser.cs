using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using Drachenkatze.PresetMagician.VSTHost.VST;
using GSF.Collections;
using ICSharpCode.SharpZipLib.Zip.Compression;
using PresetMagician.Models;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.DmitrySches
{
    public abstract class DmitrySchesPresetParser : RecursiveBankDirectoryParser
    {
        private byte[] _decodeBuffer;
        private const int DecodeBufferSize = 1024 * 1024 * 100;

        public override void Init()
        {
            _decodeBuffer = new byte[DecodeBufferSize];
            base.Init();
        }

        protected override byte[] ProcessFile(string fileName, Preset preset)
        {
            var inflater = new Inflater(false);
            inflater.SetInput(File.ReadAllBytes(fileName));
            var size = inflater.Inflate(_decodeBuffer);

            return _decodeBuffer.GetRange(0, size).ToArray();
        }
    }
}