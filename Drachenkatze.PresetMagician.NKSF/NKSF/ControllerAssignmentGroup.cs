using MessagePack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    [MessagePackObject]
    public class ControllerAssignmentGroup
    {
        public ObservableCollection<ControllerAssignment> controllerAssignments;

        public ControllerAssignmentGroup ()
        {
            controllerAssignments = new ObservableCollection<ControllerAssignment>();
        }
    }
}
