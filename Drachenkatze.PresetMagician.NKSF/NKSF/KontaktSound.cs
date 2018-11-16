using System.IO;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    public class KontaktSound : RIFFBase
    {
        public KontaktSound()
        {
            summaryInformation = new SummaryInformationChunk();
            pluginId = new PluginIdChunk();
            controllerAssignments = new ControllerAssignmentsChunk();
            pluginChunk = new PluginChunk();
        }

        public override void Read(Stream source)
        {
            summaryInformation.Read(source);
            controllerAssignments.Read(source);
            pluginId.Read(source);
            pluginChunk.Read(source);
        }

        public override void Write(Stream target)
        {
            summaryInformation.Write(target);
            controllerAssignments.Write(target);
            pluginId.Write(target);
            pluginChunk.Write(target);
        }

        public SummaryInformationChunk summaryInformation { get; set; }
        public PluginIdChunk pluginId { get; set; }
        public ControllerAssignmentsChunk controllerAssignments { get; set; }
        public PluginChunk pluginChunk { get; set; }
    }
}