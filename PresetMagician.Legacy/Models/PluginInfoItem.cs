using System.Runtime.Serialization;

namespace PresetMagician.Legacy.Models
{
    [DataContract]
    public class PluginInfoItem
    {
        public string Category { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public PluginInfoItem()
        {
        }

        public PluginInfoItem(string category, string name, string value)
        {
            Category = category;
            Name = name;
            Value = value;
        }
    }
}