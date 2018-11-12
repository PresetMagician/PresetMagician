using GSF;
using GSF.Media;
using GSF.Parsing;
using System;
using System.IO;
using MessagePack;
using System.Diagnostics;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    public class PluginIdChunk : AbstractMessagePack
    {
        public const string RiffTypeID = "PLID";

        public PluginIdChunk()
        {
            pluginId = new PluginId();
        }

        public override void Read(Stream source)
        {
            base.ReadData(source, RiffTypeID);
        }

        public override void Write(Stream target)
        {
            base.WriteData(target);
        }

        public override void DeserializeMessagePack(byte[] buffer)
        {
            Debug.WriteLine(MessagePackSerializer.ToJson(buffer));

            pluginId = MessagePackSerializer.Deserialize<PluginId>(buffer);

            Debug.WriteLine(pluginId.VSTMagic);
        }

        public PluginId pluginId;

        public override string ChunkDescription
        {
            get
            {
                return "Native Instruments Plugin ID";
            }
        }
    }
}