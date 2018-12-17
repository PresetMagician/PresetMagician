using System;
using System.Collections.ObjectModel;
using Catel.Data;
using Catel.Runtime.Serialization;
using Newtonsoft.Json;
using PresetMagicianShell.Models.Settings;

namespace PresetMagicianShell.Models
{
    [JsonObjectAttribute(MemberSerialization.OptIn)]
    public class RuntimeConfiguration: ModelBase
    {
        [JsonProperty]
        public ObservableCollection<VstDirectory> VstDirectories { get; set; } = new ObservableCollection<VstDirectory>();
        [JsonProperty]
        public ObservableCollection<Plugin> Plugins { get; set; } = new ObservableCollection<Plugin>();
    }
}