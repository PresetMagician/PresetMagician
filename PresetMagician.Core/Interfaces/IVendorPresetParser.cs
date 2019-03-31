using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PresetMagician.Core.Models;
using PresetMagician.Utils.Logger;

namespace PresetMagician.Core.Interfaces
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
        PresetParserConfiguration PresetParserConfiguration { get; set; }

        bool RequiresRescan();
        int GetNumPresets();

        bool CanHandle();
        void Init();

        ObservableCollection<ObservableCollection<string>> DefaultTypes { get; set; }
        ObservableCollection<string> DefaultModes { get; set; }

        IDataPersistence DataPersistence { get; set; }

        List<int> GetSupportedPlugins();
        MiniLogger Logger { get; }

        Task DoScan();
        void OnAfterPresetExport();
    }
}