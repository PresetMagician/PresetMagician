using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PresetMagician.SharedModels;

namespace SharedModels
{
    public interface IVendorPresetParser
    {
        PresetBank RootBank { get; set; }
        ObservableCollection<Preset> Presets { get; set; }
        IRemotePluginInstance PluginInstance { get; set; }
        int AudioPreviewPreDelay { get; set; }

        bool RequiresLoadedPlugin { get; }

        bool SupportsAdditionalBankFiles { get; set; }

        List<BankFile> AdditionalBankFiles { get; }

        string Remarks { get; set; }
        string PresetParserType { get; }

        string BankLoadingNotes { get; set; }
        bool IsNullParser { get; }
        int GetNumPresets();

        bool CanHandle();
        void Init();

        ObservableCollection<ObservableCollection<string>> DefaultTypes { get; set; }
        ObservableCollection<string> DefaultModes { get; set; }

        IPresetDataStorer PresetDataStorer { get; set; }

        List<int> GetSupportedPlugins();
        void ScanBanks();
        Task DoScan();
        void OnAfterPresetExport();
    }
}