using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    public abstract class AudioThing : AbstractVendorPresetParser
    {
        protected abstract string GetDataDirectory();

        public override void Init()
        {
            BankLoadingNotes = $"Presets are loaded from {GetDataDirectory()}";
        }

        public override async Task DoScan()
        {
            var vc2parser = new VC2Parser(Plugin, "atp", PresetDataStorer);
            await vc2parser.DoScan(RootBank, GetDataDirectory());
        }
    }
}