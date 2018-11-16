using GSF;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

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

        public override void WriteChunk()
        {
            summaryInformation.WriteChunk();
            controllerAssignments.WriteChunk();
            pluginId.WriteChunk();
            pluginChunk.WriteChunk();

            MemoryStream memoryStream = new MemoryStream();
            summaryInformation.getData().WriteTo(memoryStream);
            controllerAssignments.getData().WriteTo(memoryStream);
            pluginId.getData().WriteTo(memoryStream);
            pluginChunk.getData().WriteTo(memoryStream);

            Chunk = memoryStream.ToArray();


        }

        public override void Write(Stream target)
        {
            byte[] buffer = new byte[1];
            target.Write(Chunk, 0, Chunk.Length);
            buffer[0] = 0x0;

            if (ChunkSize % 2 != 0)
            {
                target.Write(buffer, 0, 1);
            }

        }

        public SummaryInformationChunk summaryInformation { get; set; }
        public PluginIdChunk pluginId { get; set; }
        public ControllerAssignmentsChunk controllerAssignments { get; set; }
        public PluginChunk pluginChunk { get; set; }
    }
}