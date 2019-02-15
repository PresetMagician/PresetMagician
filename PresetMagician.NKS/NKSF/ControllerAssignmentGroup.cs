using System.Collections.ObjectModel;
using MessagePack;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    [MessagePackObject]
    public class ControllerAssignmentGroup
    {
        public ObservableCollection<ControllerAssignment> controllerAssignments;

        public ControllerAssignmentGroup()
        {
            controllerAssignments = new ObservableCollection<ControllerAssignment>();
        }
    }
}