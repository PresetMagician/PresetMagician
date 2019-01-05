using System.Collections.ObjectModel;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public interface IVendorPresetParser
    {
        PresetBank RootBank { get; }
        ObservableCollection<Preset> Presets { get; }
        IVstPlugin VstPlugin { get; set; }
        int AudioPreviewPreDelay { get; set; }
        string Remarks { get; set; }
        string PresetParserType { get; }
        bool IsNullParser { get; }

        bool CanHandle();
        
        ObservableCollection<ObservableCollection<string>> DefaultTypes { get; set; }

        ObservableCollection<string> DefaultModes { get; set; }


        void ScanBanks();
        void OnAfterPresetExport(VstHost host, IVstPlugin plugin);
    }
}