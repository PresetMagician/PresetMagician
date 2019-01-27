using Drachenkatze.PresetMagician.NKSF.NKSF;

namespace PresetMagician.Models.ControllerAssignments
{
    public class ControllerAssignmentControl : ControllerAssignment
    {
        public ControllerAssignmentControl(ControllerAssignment baseObject)
        {
            autoname = baseObject.autoname;
            id = baseObject.id;
            name = baseObject.name;
            vflag = baseObject.vflag;
            section = baseObject.section;
        }

        public bool LastSectionItem { get; set; }
    }
}