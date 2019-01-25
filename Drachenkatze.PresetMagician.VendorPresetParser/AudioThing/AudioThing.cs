using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    public abstract class AudioThing : RecursiveBankDirectoryParser
    {
        protected override string Extension { get; } = "atp";
    }
}