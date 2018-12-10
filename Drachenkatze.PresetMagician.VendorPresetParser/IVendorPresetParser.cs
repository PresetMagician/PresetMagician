using Drachenkatze.PresetMagician.VSTHost.VST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public interface IVendorPresetParser
    {
        List<PresetBank> Banks { get; }
        IVstPlugin VstPlugin { get; set; }
        string Remarks { get; set; }
        int NumPresets { get; }
        string PresetParserType { get; }
        bool IsNullParser { get; }

        bool CanHandle();

        void ScanBanks();
        void OnAfterPresetExport(VstHost host, IVstPlugin plugin);
    }
}