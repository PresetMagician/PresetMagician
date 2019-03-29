using System;
using System.Collections.Generic;
using Ceras;
using MessagePack;

namespace PresetMagician.NKS
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