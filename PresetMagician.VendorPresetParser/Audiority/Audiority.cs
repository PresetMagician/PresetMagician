using PresetMagician.VendorPresetParser.Common;

namespace PresetMagician.VendorPresetParser.Audiority
{
    public abstract class Audiority : RecursiveVC2Parser
    {
        protected override string Extension { get; } = "aup";
    }
}