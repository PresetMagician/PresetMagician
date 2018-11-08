using GSF;
using GSF.Media;
using GSF.Parsing;
using System;
using System.IO;
using MessagePack;

namespace NKSF
{
    /// <summary>
    /// Represents the format chunk in a WAVE media format file.
    /// </summary>
    public class PluginId : AbstractMessagePack, ISupportBinaryImage
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// RIFF type ID for wave format chunk (i.e., "fmt ").
        /// </summary>
        public const string RiffTypeID = "PLID";

        #endregion

        #region [ Constructors ]

        /// <summary>Reads a new <see cref="WaveFormatChunk"/> from the specified stream.</summary>
        /// <param name="preRead">Pre-parsed <see cref="RiffChunk"/> header.</param>
        /// <param name="source">Source stream to read data from.</param>
        /// <exception cref="InvalidOperationException">WAVE format or extra parameters section too small, wave file corrupted.</exception>
        /// <exception cref="InvalidDataException">Invalid bit rate encountered - wave file bit rates must be a multiple of 8.</exception>
        public PluginId(RiffChunk preRead, Stream source)
            : base(preRead, source, RiffTypeID)
        {
            ReadMessagePack(preRead, source);
        }

        #endregion

        #region [ Properties ]

        public override string ChunkDescription
        {
            get
            {
                return "Native Instruments Plugin ID";
            }

        }
        #endregion
    }


}