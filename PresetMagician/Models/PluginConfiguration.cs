using System.Collections.Generic;
using System.Collections.ObjectModel;
using Catel.Data;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Newtonsoft.Json;

namespace PresetMagician.Models
{
    public class PluginConfiguration : ModelBase, IPluginConfiguration
    {
        [JsonProperty] public bool IsEnabled { get; set; } = true;
        [JsonProperty] public bool IsReported { get; set; }
        [JsonProperty] public int AudioPreviewPreDelay { get; set; }
        [JsonProperty] public Drachenkatze.PresetMagician.NKSF.NKSF.ControllerAssignments DefaultControllerAssignments { get; set; }
        [JsonProperty] public ObservableCollection<BankFile> AdditionalBankFiles { get; } = new ObservableCollection<BankFile>();
    }
}