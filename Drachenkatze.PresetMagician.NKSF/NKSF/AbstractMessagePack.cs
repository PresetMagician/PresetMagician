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
        public abstract byte[] SerializeMessagePack();

        public override void WriteChunk()
        {
            MemoryStream ms = new MemoryStream();
            byte[] tmpChunk = SerializeMessagePack();

            Chunk = new byte[tmpChunk.Length + 4];
            ms.Write(LittleEndian.GetBytes(Version),0,4);
            ms.Write(tmpChunk, 0, tmpChunk.Length);
            Chunk = ms.ToArray();
            ChunkSize = Chunk.Length;
        }
    }
}