using System.Collections.Generic;
using System.Xml.Serialization;
using Ceras;
using SharedModels.Models;

namespace SharedModels
{
    public class Type: TrackableModelBaseFoo
    {
        [System.ComponentModel.DataAnnotations.Key] public int Id { get; set; }

        [XmlIgnore] [Exclude] public ICollection<Plugin> Plugins { get; set; }
        [XmlIgnore] [Exclude] public ICollection<Preset> Presets { get; set; }

        public string Name { get; set; }
        public string SubTypeName { get; set; }
    }
}