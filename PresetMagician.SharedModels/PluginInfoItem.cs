using System.Runtime.Serialization;

namespace PresetMagician.Models
{
    [DataContract]
    public class PluginInfoItem
    {
        [DataMember] public string Category { get; set; }

        [DataMember] public string Name { get; set; }

        [DataMember] public string Value { get; set; }

        public PluginInfoItem(string category, string name, string value)
        {
            Category = category;
            Name = name;
            Value = value;
        }
    }
}