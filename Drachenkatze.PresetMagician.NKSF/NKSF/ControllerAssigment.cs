using System;
using MessagePack;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    [MessagePackObject]
    [Serializable]
    public class ControllerAssignment
    {
        [Key("autoname")] public bool autoname { get; set; }

        [Key("id")] public int? id { get; set; }

        [Key("name")] public string name { get; set; }

        [Key("vflag")] public bool vflag { get; set; }

        [Key("section")] public string section { get; set; }
    }
}