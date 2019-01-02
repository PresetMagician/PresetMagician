using Drachenkatze.PresetMagician.NKSF.NKSF;

namespace PresetMagician.Models
{
    public interface IPluginConfiguration
    {
        bool IsEnabled { get; set; }
        int AudioPreviewPreDelay { get; set; }
        ControllerAssignments DefaultControllerAssignments { get; set; }
        bool IsReported { get; set; }
    }
}