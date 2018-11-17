using System.Diagnostics;
using System.IO;
using MessagePack;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    /// <summary>
    /// Represents the format chunk in a WAVE media format file.
    /// </summary>
    public class SummaryInformationChunk : AbstractMessagePack
    {
        public const string RiffTypeID = "NISI";

        public SummaryInformationChunk()
        {
            summaryInformation = new SummaryInformation();
            TypeID = RiffTypeID;
            Version = 1;
        }

        public override void Write(Stream target)
        {
            base.WriteData(target);
        }

        public override void Read(Stream source)
        {
            base.ReadData(source, RiffTypeID);
        }

        public override void DeserializeMessagePack(byte[] buffer)
        {
            summaryInformation = MessagePackSerializer.Deserialize<SummaryInformation>(buffer);

            Debug.WriteLine(MessagePackSerializer.ToJson(buffer));
            Debug.WriteLine(summaryInformation.author);
            Debug.WriteLine(summaryInformation.comment);
            Debug.WriteLine(summaryInformation.deviceType);
            Debug.WriteLine(summaryInformation.name);
            Debug.WriteLine(summaryInformation.uuid);
            Debug.WriteLine(summaryInformation.vendor);
            Debug.WriteLine(summaryInformation.bankChain.Count);
            Debug.WriteLine(summaryInformation.bankChain[0]);
        }

        public string getJSON()
        {
            return MessagePackSerializer.ToJson(MessagePackSerializer.Serialize(summaryInformation));
        }

        public override byte[] SerializeMessagePack()
        {
            byte[] b = MessagePackSerializer.Serialize(summaryInformation);

            Debug.WriteLine(MessagePackSerializer.ToJson(b));
            return b;
        }

        public SummaryInformation summaryInformation;

        public override string ChunkDescription
        {
            get
            {
                return "Native Instruments Summary Information";
            }
        }
    }
}