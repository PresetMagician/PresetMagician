using System;
using Ceras;
using MessagePack;

namespace PresetMagician.NKS
{
    [MessagePackObject]
    [Serializable]
    public class ControllerAssignment
    {
        [Key("autoname")] [Include] public bool autoname { get; set; }

        [Key("id")] [Include] public int? id { get; set; }

        [Key("name")] [Include] public string name { get; set; }

        [Key("vflag")] [Include] public bool vflag { get; set; }

        [Key("section")] [Include] public string section { get; set; }
    }
}