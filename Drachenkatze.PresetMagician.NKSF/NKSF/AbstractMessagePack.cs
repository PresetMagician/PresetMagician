using System;
using System.IO;
using GSF;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    /// <summary>
    /// Represents the format chunk in a WAVE media format file.
    /// </summary>
    abstract public class AbstractMessagePack : RIFFBase
    {
        public long Version;
        public virtual string ChunkDescription { get; }

        public override void ReadData(Stream source)
        {
            base.ReadData(source);

            int length = ChunkSize;

            byte[] buffer = new byte[length - 4];
            byte[] version = new byte[4];

            Version = BitConverter.ToInt32(Chunk.BlockCopy(0, 4), 0);
            buffer = Chunk.BlockCopy(4, length - 4);

            DeserializeMessagePack(buffer);
        }

        public abstract void DeserializeMessagePack(byte[] buffer);

        public override void WriteData(Stream target)
        {
            // Process mesgpack here
            base.WriteData(target);
        }
    }
}