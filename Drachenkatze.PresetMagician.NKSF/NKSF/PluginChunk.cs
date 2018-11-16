using GSF;
using System;
using System.Diagnostics;
using System.IO;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    public class PluginChunk : RIFFBase
    {
        public const string RiffTypeID = "PCHK";
        public long Version;
        public byte[] PresetData;
        public PluginChunk ()
        {
            TypeID = RiffTypeID;
            Chunk = new byte[0];
            Version = 1;
            PresetData = new byte[0];
        }
           
        public override void Read(Stream source)
        {
            base.ReadData(source, RiffTypeID);

            int length = ChunkSize;

            PresetData = new byte[length - 4];
            byte[] version = new byte[4];

            Version = BitConverter.ToInt32(Chunk.BlockCopy(0, 4), 0);
            PresetData = Chunk.BlockCopy(4, length - 4);
        }

        public override void WriteChunk()
        {
            MemoryStream ms = new MemoryStream();
            byte[] tmpChunk = PresetData;

            Chunk = new byte[tmpChunk.Length + 4];
            ms.Write(LittleEndian.GetBytes(Version), 0, 4);
            ms.Write(tmpChunk, 0, tmpChunk.Length);
            Chunk = ms.ToArray();
            ChunkSize = Chunk.Length;
        }

        public override void Write(Stream target)
        {
            Debug.WriteLine("Writing PluginChunk");
            base.WriteData(target);
        }
    }
}