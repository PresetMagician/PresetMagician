using GSF;
using GSF.Media;
using GSF.Parsing;
using System;
using System.IO;
using MessagePack;
using System.Diagnostics;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    /// <summary>
    /// Represents the format chunk in a WAVE media format file.
    /// </summary>
    public class SummaryInformationChunk : AbstractMessagePack
    {
        public const string RiffTypeID = "NISI";

        public override void Read(Stream source)
        {
            summaryInformation = new SummaryInformation();
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

        public SummaryInformation summaryInformation;

        public override void Write(Stream target)
        {
            base.WriteData(target);
        }

        public override string ChunkDescription
        {
            get
            {
                return "Native Instruments Summary Information";
            }
        }
    }
}