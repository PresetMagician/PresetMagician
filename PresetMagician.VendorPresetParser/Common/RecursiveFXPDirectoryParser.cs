using Drachenkatze.PresetMagician.Utils;
using PresetMagician.Core.Models;

namespace PresetMagician.VendorPresetParser.Common
{
    public abstract class RecursiveFXPDirectoryParser : RecursiveBankDirectoryParser
    {
        protected override byte[] ProcessFile(string fileName, PresetParserMetadata preset)
        {
            var fxp = new FXP();
            fxp.ReadFile(fileName);

            return fxp.ChunkDataByteArray;
        }
    }
}