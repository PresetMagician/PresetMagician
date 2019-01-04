using Drachenkatze.PresetMagician.VendorPresetParser.Common;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    public abstract class AudioThing: AbstractVendorPresetParser
    {
        protected void DoScan(PresetBank rootBank, string directory)
        {
            var vc2parser = new VC2Parser(VstPlugin, "atp",Presets);
            vc2parser.DoScan(rootBank, directory);
        }
    }
}
