using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using GSF;

namespace PresetMagician.NKS
{
    public class NKSFRiff : RIFFBase
    {
        private const int m_headerLength = 12;

        public NKSFRiff()
        {
            kontaktSound = new KontaktSound();
            TypeID = "RIFF";
            Chunk = Encoding.ASCII.GetBytes(TypeID);
            m_FileFormat = "NIKS";
        }

        public override void Read(Stream source)
        {
            byte[] padding = new byte[1];

            byte[] buffer = new byte[m_headerLength];

            int bytesRead = source.Read(buffer, 0, 12);

            if (bytesRead < m_headerLength)
                throw new InvalidOperationException("RIFF chunk too small, media file corrupted");

            TypeID = Encoding.ASCII.GetString(buffer, 0, 4);
            ChunkSize = LittleEndian.ToInt32(buffer, 4);
            Debug.WriteLine("Chunk size is " + ChunkSize.ToString());
            Chunk = new byte[4];
            Chunk = buffer.BlockCopy(8, 4);

            kontaktSound.Read(source);
        }

        public override void WriteChunk()
        {
        }

        public override void Write(Stream target)
        {
            this.WriteData(target);
            kontaktSound.Write(target);
        }

        public override void WriteData(Stream target)
        {
            Debug.WriteLine("Writing NKSFRIFF");
            byte[] buffer = new byte[m_headerLength];

            Buffer.BlockCopy(Encoding.ASCII.GetBytes(TypeID), 0, buffer, 0, 4);

            kontaktSound.WriteChunk();

            Buffer.BlockCopy(LittleEndian.GetBytes(kontaktSound.Chunk.Length + 4), 0, buffer, 4, 4);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(FileFormat), 0, buffer, 8, 4);
            target.Write(buffer, 0, 12);
        }

        public KontaktSound kontaktSound { get; set; }

        private string m_FileFormat;

        public string FileFormat
        {
            get { return m_FileFormat; }

            set
            {
                if (value != "NIKS")
                {
                    throw new ArgumentException("File format is not NKSF (code NIKS)");
                }

                m_FileFormat = value;
            }
        }
    }
}