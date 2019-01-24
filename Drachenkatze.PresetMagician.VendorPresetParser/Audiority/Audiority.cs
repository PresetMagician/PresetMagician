using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Audiority
{
    public abstract class Audiority : AbstractVendorPresetParser
    {
        protected abstract string GetDataDirectory();

        public void Init()
        {
            BankLoadingNotes = $"Presets are loaded from {GetDataDirectory()}";
        }

        public override async Task DoScan()
        {
            var vc2parser = new VC2Parser(Plugin, "aup", PresetDataStorer);
            await vc2parser.DoScan(RootBank, GetDataDirectory());
        }
    }
}