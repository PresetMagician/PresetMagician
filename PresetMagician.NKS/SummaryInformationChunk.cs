using System;
using System.Diagnostics;
using System.IO;
using MessagePack;

namespace PresetMagician.NKS
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
            try
            {
                summaryInformation = MessagePackSerializer.Deserialize<SummaryInformation>(buffer);

            }
            catch (Exception)
            {
            }
        }

        public string getJSON()
        {
            return MessagePackSerializer.ToJson(MessagePackSerializer.Serialize(summaryInformation));
        }

        public override byte[] SerializeMessagePack()
        {
            byte[] b = MessagePackSerializer.Serialize(summaryInformation);

            return b;
        }

        public SummaryInformation summaryInformation;

        public override string ChunkDescription
        {
            get { return "Native Instruments Summary Information"; }
        }
    }
}