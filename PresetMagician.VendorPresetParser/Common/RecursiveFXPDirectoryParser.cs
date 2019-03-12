using Drachenkatze.PresetMagician.Utils;
using PresetMagician.Core.Models;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Common
{
    public abstract class RecursiveFXPDirectoryParser : RecursiveBankDirectoryParser
    {
        protected override byte[] ProcessFile(string fileName, Preset preset)
        {
            var fxp = new FXP();
            fxp.ReadFile(fileName);

            return fxp.ChunkDataByteArray;
        }
    }
}