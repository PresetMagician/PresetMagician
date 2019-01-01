using MessagePack;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    [MessagePackObject]
    public class ControllerAssignments
    {
        [Key("ni8")] public List<List<ControllerAssignment>> controllerAssignments { get; set; }

        public ControllerAssignments()
        {
            controllerAssignments = new List<List<ControllerAssignment>>();
        }
    }
}