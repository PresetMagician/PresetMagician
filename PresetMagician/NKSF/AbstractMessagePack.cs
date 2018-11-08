using GSF;
using GSF.Media;
using GSF.Parsing;
using System;
using System.IO;
using MessagePack;
using System.Diagnostics;

namespace NKSF
{
    /// <summary>
    /// Represents the format chunk in a WAVE media format file.
    /// </summary>
    abstract public class AbstractMessagePack : RiffChunk, ISupportBinaryImage
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// RIFF type ID for wave format chunk (i.e., "fmt ").
        /// </summary>
        public long m_version;
        private string m_type;
        public virtual string ChunkDescription { get; }

        #endregion

        #region [ Constructors ]

        /// <summary>Reads a new <see cref="WaveFormatChunk"/> from the specified stream.</summary>
        /// <param name="preRead">Pre-parsed <see cref="RiffChunk"/> header.</param>
        /// <param name="source">Source stream to read data from.</param>
        /// <exception cref="InvalidOperationException">WAVE format or extra parameters section too small, wave file corrupted.</exception>
        /// <exception cref="InvalidDataException">Invalid bit rate encountered - wave file bit rates must be a multiple of 8.</exception>
        public AbstractMessagePack(RiffChunk preRead, Stream source, string type)
            : base(preRead, type)
        {
            m_type = type;
        }

        #endregion

        #region [ Properties ]

        public void ReadMessagePack(RiffChunk preRead, Stream source)
        {
            int length = ChunkSize;

            byte[] buffer = new byte[length - 4];
            byte[] version = new byte[4];

            Debug.WriteLine(string.Format("fourCC: {0} {1}", m_type, ChunkDescription));

            try
            {
                source.Read(version, 0, 4);
                m_version = BitConverter.ToInt32(version, 0);
                
                Debug.WriteLine("Version: {0}", m_version);
                int bytesRead = source.Read(buffer, 0, length - 4);

                var json = MessagePackSerializer.ToJson(buffer);
                Debug.WriteLine(string.Format("MsgPack JSON string: {0}", json));
                // Initialize class from buffer
                // ParseBinaryImage(buffer, 0, bytesRead);

            }
            finally
            {
                if (ChunkSize % 2 != 0)
                {
                    source.Read(buffer, 0, 1);
                }
            }
        }
        /// <summary>Size of <see cref="WaveFormatChunk"/>.</summary>
        public override int ChunkSize
        {
            get
            {
                // Trust the read size over the typical constants if available
                int chunkSize = base.ChunkSize;

                return chunkSize;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the length of <see cref="WaveFormatChunk"/>.
        /// </summary>
        public override int BinaryLength
        {
            get
            {
                return base.BinaryLength + ChunkSize;
            }
        }

        #endregion

        #region [ Methods ]

        #endregion
    }
}