using Drachenkatze.PresetMagician.VendorPresetParser.Common;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    public abstract class AudioThing : RecursiveBankDirectoryParser
    {
        protected override string Extension { get; } = "atp";
    }
}