using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Catel.Data;

namespace SharedModels
{
    public class Type: TrackableModelBase
    {
        [Key] public int Id { get; set; }

        [XmlIgnore] public ICollection<Plugin> Plugins { get; set; }
        [XmlIgnore] public ICollection<Preset> Presets { get; set; }

        public string Name { get; set; }
        public string SubTypeName { get; set; }
    }
}