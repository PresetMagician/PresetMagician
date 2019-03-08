using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SharedModels
{
    public interface IVendorPresetParser
    {
        PresetBank RootBank { get; set; }
        IRemotePluginInstance PluginInstance { get; set; }
        int AudioPreviewPreDelay { get; set; }

        string Remarks { get; set; }
        string PresetParserType { get; }

        string BankLoadingNotes { get; set; }
        bool IsNullParser { get; }
        int GetNumPresets();

        bool CanHandle();
        void Init();

        ObservableCollection<ObservableCollection<string>> DefaultTypes { get; set; }
        ObservableCollection<string> DefaultModes { get; set; }

        IDataPersistence DataPersistence { get; set; }

        List<int> GetSupportedPlugins();
      
        Task DoScan();
        void OnAfterPresetExport();
    }
}