using System;
using System.Collections.Generic;
using Ceras;
using MessagePack;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    [MessagePackObject]
    [Serializable]
    public class ControllerAssignments
    {
        [Key("ni8")] [Include] public List<List<ControllerAssignment>> controllerAssignments { get; set; }

        public ControllerAssignments()
        {
            controllerAssignments = new List<List<ControllerAssignment>>();
        }
    }
}