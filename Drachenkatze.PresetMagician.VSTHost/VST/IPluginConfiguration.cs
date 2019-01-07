using System.Collections.Generic;
using System.Collections.ObjectModel;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace PresetMagician.Models
{
    public interface IPluginConfiguration
    {
        bool IsEnabled { get; set; }
        int AudioPreviewPreDelay { get; set; }
        ControllerAssignments DefaultControllerAssignments { get; set; }
        bool IsReported { get; set; }
        ObservableCollection<BankFile> AdditionalBankFiles { get; }
    }
}