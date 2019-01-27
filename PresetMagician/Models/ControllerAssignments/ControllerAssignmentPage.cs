using System.Collections.ObjectModel;

namespace PresetMagician.Models.ControllerAssignments
{
    public class ControllerAssignmentPage
    {
        public string Title { get; set; }

        public ObservableCollection<ControllerAssignmentControl> Controls { get; set; } =
            new ObservableCollection<ControllerAssignmentControl>();
    }
}