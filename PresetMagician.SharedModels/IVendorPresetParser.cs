using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagician.Models;
using PresetMagician.SharedModels;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public interface IVendorPresetParser
    {
        PresetBank RootBank { get; set; }
        ObservableCollection<Preset> Presets { get; set; }
        Plugin Plugin { get; set; }
        int AudioPreviewPreDelay { get; set; }
        
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
        IRemoteVstService RemoteVstService { get; set; }
        List<int> GetSupportedPlugins();
        void ScanBanks();
        Task DoScan();
        void OnAfterPresetExport();
    }
}