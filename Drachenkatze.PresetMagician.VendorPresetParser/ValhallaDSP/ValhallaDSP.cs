using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.ValhallaDSP
{
    public abstract class ValhallaDSP : AbstractVendorPresetParser
    {
        protected abstract string GetDataDirectory();

        public override void Init()
        {
             BankLoadingNotes =
            $"Presets are loaded from {GetDataDirectory()}. First sub-folder defines the bank.";
        }

        public override async Task DoScan()
        {
            var parser = new VC2Parser(Plugin, "vpreset", PresetDataStorer);
            await parser.DoScan(RootBank, GetDataDirectory());
        }
    }
}