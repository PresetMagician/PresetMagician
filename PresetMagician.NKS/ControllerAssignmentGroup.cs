using System.Collections.ObjectModel;
using MessagePack;

namespace PresetMagician.NKS
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