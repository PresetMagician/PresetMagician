using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Audiority
{
    public abstract class Audiority : RecursiveVC2Parser
    {
        protected override string Extension { get; } = "aup";
    }
}