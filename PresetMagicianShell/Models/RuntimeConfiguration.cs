using System;
using System.Collections.ObjectModel;
using Catel.Collections;
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
        public FastObservableCollection<VstDirectory> VstDirectories { get; set; } = new FastObservableCollection<VstDirectory>();
        [JsonProperty]
        public FastObservableCollection<Plugin> Plugins { get; set; } = new FastObservableCollection<Plugin>();
    }
}