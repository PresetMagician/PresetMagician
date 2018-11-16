using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using GSF;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    abstract public class RIFFBase
    {
        private const int m_length = 8;

        public RIFFBase ()
        {
            Chunk = new byte[0];
        }

        abstract public void Read(Stream source);

        abstract public void Write(Stream target);

        abstract public void WriteChunk();

        public virtual void ReadData(Stream source)
        {
            byte[] padding = new byte[1];

            byte[] buffer = new byte[m_length];

            int bytesRead = source.Read(buffer, 0, 8);

            if (bytesRead < m_length)
                throw new InvalidOperationException("RIFF chunk too small, media file corrupted");

            TypeID = Encoding.ASCII.GetString(buffer, 0, 4);
            ChunkSize = LittleEndian.ToInt32(buffer, 4);

            Chunk = new byte[ChunkSize];

            source.Read(Chunk, 0, ChunkSize);

            if (ChunkSize % 2 != 0)
            {
                source.Read(padding, 0, 1);
            }

            Debug.WriteLine("Read chunk ID " + TypeID);
        }

        public virtual void WriteData(Stream target)
        {
            byte[] buffer = new byte[8];

            Buffer.BlockCopy(Encoding.ASCII.GetBytes(TypeID), 0, buffer, 0, 4);
            Buffer.BlockCopy(LittleEndian.GetBytes(ChunkSize), 0, buffer, 4, 4);
            target.Write(buffer, 0, 8);

            target.Write(Chunk, 0, Chunk.Length);
            buffer[0] = 0x0;

            if (ChunkSize % 2 != 0)
            {
                target.Write(buffer, 0, 1);
            }
        }

        public void ReadData(Stream source, String ExpectedTypeId)
        {
            ReadData(source);
            if (TypeID != ExpectedTypeId)
            {
                throw new ArgumentException("Problem in file detected: Expected " + ExpectedTypeId + " got " + TypeID);
            }
        }

        public MemoryStream getData ()
        {
            MemoryStream target = new MemoryStream();

            byte[] buffer = new byte[8];

            Buffer.BlockCopy(Encoding.ASCII.GetBytes(TypeID), 0, buffer, 0, 4);
            Buffer.BlockCopy(LittleEndian.GetBytes(ChunkSize), 0, buffer, 4, 4);
            target.Write(buffer, 0, 8);

            target.Write(Chunk, 0, Chunk.Length);
            buffer[0] = 0x0;

            if (ChunkSize % 2 != 0)
            {
                target.Write(buffer, 0, 1);
            }

            return target;
        }

        public byte[] Chunk { get; set; }

        // Fields
        public string TypeID { get; set; }

        public int ChunkSize { get; set; }
    }
}