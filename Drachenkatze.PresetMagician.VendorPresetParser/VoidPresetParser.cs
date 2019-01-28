using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    [UsedImplicitly]
    public class VoidPresetParser : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override string Remarks { get; set; } =
            "Plugin doesn't seem to have preset loading/saving capabilities";

        public override List<int> SupportedPlugins => new List<int>
        {
            1951355500,
            1919243824, // u-he Zrev
            1364228170,
            1098206310, // Audiority Low Filter
            1098208102, // Audiority Side Filter
            1483101268, // Xfer OTT
            1936223602, // ValhallaDSP Shimmer
            1181828456, // ValhallaDSP FreqEcho
            1919243828, // TAL Reverb 4
            1665682481, // Tal Chorus LX
            1296985972, // MeldaProduction MNotepad
            1297052499, // MeldaProduction Oscilloscope
            1297245539, // MeldaProduction Recorder
            1297306467, // MeldaProduction StereoScope
            1297380718, // MeldaProduction Tuner

            0
        };

        public override async Task DoScan()
        {
        }

        public override int GetNumPresets()
        {
            return 0;
        }

        public void ScanBanks()
        {
        }
    }
}