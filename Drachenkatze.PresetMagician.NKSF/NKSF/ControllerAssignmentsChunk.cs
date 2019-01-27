using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MessagePack;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    public class ControllerAssignmentsChunk : AbstractMessagePack
    {
        public const string RiffTypeID = "NICA";

        public ControllerAssignmentsChunk()
        {
            TypeID = RiffTypeID;
            Version = 1;
            ControllerAssignment cv = new ControllerAssignment();
            cv.name = "bla";
            cv.id = 0;
            cv.autoname = false;
            cv.vflag = false;
            controllerAssignments = new ControllerAssignments();
            controllerAssignments.controllerAssignments.Add(new List<ControllerAssignment> {cv});
        }

        public override void Read(Stream source)
        {
            base.ReadData(source, RiffTypeID);
        }

        public override void Write(Stream target)
        {
            base.WriteData(target);
        }

        public ControllerAssignments controllerAssignments;

        public override void DeserializeMessagePack(byte[] buffer)
        {
            Debug.WriteLine(MessagePackSerializer.ToJson(buffer));
            controllerAssignments = MessagePackSerializer.Deserialize<ControllerAssignments>(buffer);
        }

        public override byte[] SerializeMessagePack()
        {
            byte[] b = MessagePackSerializer.Serialize(controllerAssignments);

            Debug.WriteLine(MessagePackSerializer.ToJson(b));
            return b;
        }

        public string getJSON()
        {
            return MessagePackSerializer.ToJson(MessagePackSerializer.Serialize(controllerAssignments));
        }

        public override string ChunkDescription
        {
            get { return "Native Instruments Controller Assignments"; }
        }
    }
}