using System.Runtime.Serialization;
using Ceras;

namespace SharedModels
{
    [DataContract]
    public class PluginInfoItem
    {
        [DataMember] [Include] public string Category { get; set; }

        [DataMember] [Include] public string Name { get; set; }

        [DataMember] [Include] public string Value { get; set; }

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