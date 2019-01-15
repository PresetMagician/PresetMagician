using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    public abstract class AudioThing: AbstractVendorPresetParser
    {
        protected void DoScan(PresetBank rootBank, string directory)
        {
            var vc2parser = new VC2Parser(Plugin, "atp",Presets);
            vc2parser.DoScan(rootBank, directory);
        }
    }
}
