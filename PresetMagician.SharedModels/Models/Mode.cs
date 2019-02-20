using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Catel.Data;
using Catel.Fody;

namespace SharedModels
{
    public class Mode:TrackableModelBase
    {
        [Key] public int Id { get; set; }

        [XmlIgnore] public ICollection<Plugin> Plugins { get; set; }
        [XmlIgnore] public ICollection<Preset> Presets { get; set; }

        public string Name { get; set; }
    }
}