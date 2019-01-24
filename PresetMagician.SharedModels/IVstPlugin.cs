using System;
using System.Collections.ObjectModel;
using Jacobi.Vst.Interop.Host;
using PresetMagician.SharedModels;

namespace SharedModels
{
    public interface IVstPlugin
    {
        
        void OnLoaded();
        void OnLoadError(Exception e);
        int PluginId { get; set; }
        string PluginName { get; }
        Plugin.PluginTypes PluginType { get; set; } 
        int NumPresets { get; }
        string PluginVendor { get; set; }
        int PresetParserAudioPreviewPreDelay { get; }
        bool IsScanned { get; set; }
        
        bool IsEnabled { get; set; }
        int AudioPreviewPreDelay { get; set; }
        Drachenkatze.PresetMagician.NKSF.NKSF.ControllerAssignments DefaultControllerAssignments { get; set; }
        bool IsReported { get; set; }
        ObservableCollection<BankFile> AdditionalBankFiles { get; }
    }
}