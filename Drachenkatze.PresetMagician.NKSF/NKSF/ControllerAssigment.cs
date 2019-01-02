using MessagePack;
using System;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    [MessagePackObject]
    public class ControllerAssignment
    {
        [Key("autoname")]
        public Boolean autoname { get; set; }

        [Key("id")] public int id { get; set; }

        [Key("name")] public String name { get; set; }

        [Key("vflag")]
        public Boolean vflag { get; set; }

        [Key("section")]
        public String section { get; set; }
    }
}