using MessagePack;
using System;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    [MessagePackObject]
    public class ControllerAssignment
    {
        [Key("autoname")]
        public Boolean autoname;

        [Key("id")]
        public int id;

        [Key("name")]
        public String name;

        [Key("vflag")]
        public Boolean vflag;

        [Key("section")]
        public String section;

    }
}