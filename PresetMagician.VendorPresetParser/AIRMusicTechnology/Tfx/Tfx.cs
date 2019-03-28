using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GSF;
using PresetMagician.Utils;
using Catel.IO;
using Path = Catel.IO.Path;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public abstract class Tfx
    {
        public readonly List<double> Parameters = new List<double>();

        public byte[] EndChunk;
        public byte[] PatchName;

        public List<WzooBlock> WzooBlocks;
        public abstract byte[] WzooPluginId { get; }


        public abstract byte[] BlockMagic { get; }
        public MagicBlock MidiBlock { get; set; }
        public MagicBlock WzooBlock { get; set; }

        public virtual byte[] GetDataToWrite()
        {
            var parameterBytes = GetParameters();
            var blockData = GetBlockDataToWrite();

            var totalLength = parameterBytes.Length +
                              blockData.Length +
                              EndChunk.Length; // end chunk

            var ms = new MemoryStream();

            // total length
            ms.Write(LittleEndian.GetBytes(totalLength), 0, 4);

            // Fill up
            for (var i = 0; i < 12; i++)
            {
                ms.WriteByte(0);
            }

            ms.Write(parameterBytes, 0, parameterBytes.Length);
            ms.Write(blockData, 0, blockData.Length);
            ms.Write(EndChunk, 0, EndChunk.Length);
            var data = ms.ToByteArray();

            ms.Close();

            return data;
        }

        public virtual void Parse(string directory, string file)
        {
            var path = Path.Combine(directory, file);

            var data = File.ReadAllBytes(path);

            PatchName = Encoding.ASCII.GetBytes(file.Replace(".tfx", "").Replace("\\", "/"));

            WzooBlocks = ParseBlocks(data);

            ParseParameters();
            ParseWzoo();


            PostProcess();
        }

        public WzooBlock FindBlock(string type)
        {
            var blockBuffer = new byte[8];

            if (WzooPluginId == null || WzooPluginId.Length != 4)
            {
                throw new Exception("WzooPluginId must be length 4");
            }

            for (var i = 0; i < 4; i++)
            {
                blockBuffer[i] = WzooPluginId[i];
            }

            var typeBytes = Encoding.ASCII.GetBytes(type);

            if (typeBytes.Length != 4)
            {
                throw new Exception("type must be length 4");
            }

            for (var i = 0; i < 4; i++)
            {
                blockBuffer[i + 4] = typeBytes[i];
            }

            foreach (var block in WzooBlocks)
            {
                if (block.ConfigType.SequenceEqual(blockBuffer))
                {
                    return block;
                }
            }

            return null;
        }

        public List<WzooBlock> ParseBlocks(byte[] data)
        {
            var blocks = new List<WzooBlock>();
            using (var ms = new MemoryStream(data))
            {
                while (true)
                {
                    var block = ParseBlock(ms);

                    if (block == null)
                    {
                        break;
                    }

                    blocks.Add(block);
                }
            }

            return blocks;
        }

        public virtual WzooBlock ParseBlock(MemoryStream ms)
        {
            if (ms.Position == ms.Length)
            {
                return null;
            }

            var sizeBuffer = new byte[4];

            var block = new WzooBlock();

            ms.Read(sizeBuffer, 0, 4);
            block.BlockLength = BigEndian.ToInt32(sizeBuffer, 0);

            ms.Read(sizeBuffer, 0, 4);
            block.BlockVersion = BigEndian.ToInt32(sizeBuffer, 0);

            ms.Read(block.DeviceType, 0, 8);
            ms.Read(block.ConfigType, 0, 8);

            var dataLength = block.BlockLength - 24;
            block.BlockData = new byte[dataLength];
            ms.Read(block.BlockData, 0, dataLength);

            return block;
        }

        public virtual void ParseParameters()
        {
            Parameters.Clear();
            var parameterBuffer = new byte[8];
            var parameterHeader = new byte[3];
            var magicParameter = new byte[] {0x64, 0x5f, 0x00};

            var beginIndicator = new byte[] {0x01, 0x01, 0x01, 0x01};
            var block = FindBlock("elck");

            if (block == null)
            {
                throw new Exception("Unable to find parameters block");
            }

            using (var ms = new MemoryStream(block.BlockData))
            {
                var beginBuffer = new byte[4];

                ms.Seek(32, SeekOrigin.Begin);
                ms.Read(beginBuffer, 0, 4);

                if (!beginBuffer.SequenceEqual(beginIndicator))
                {
                    throw new Exception(
                        $"Expected the begin indicator to be {StringUtils.ByteArrayToString(beginIndicator)} " +
                        $"but found {StringUtils.ByteArrayToString(beginBuffer)}");
                }

                while (true)
                {
                    if (ms.Position == ms.Length)
                    {
                        break;
                    }

                    ms.Read(parameterHeader, 0, 3);

                    if (!parameterHeader.SequenceEqual(magicParameter))
                    {
                        throw new Exception(
                            $"Expected the parameter header to be {StringUtils.ByteArrayToString(magicParameter)} " +
                            @"but found {StringUtils.ByteArrayToString(parameterHeader)}");
                    }

                    ms.Seek(1, SeekOrigin.Current);
                    ms.Read(parameterBuffer, 0, 8);
                    Parameters.Add(BigEndian.ToDouble(parameterBuffer, 0));
                }
            }
        }

        public virtual void ParseWzoo()
        {
            var block = FindBlock("Wzoo");

            if (block == null)
            {
                throw new Exception("Unable to find parameters block");
            }

            WzooBlock = ParseMagicBlock(block);
        }

        public virtual void ParseMidi()
        {
            var block = FindBlock("midi");

            if (block == null)
            {
                throw new Exception("Unable to find parameters block");
            }

            MidiBlock = ParseMagicBlock(block);
        }

        public MagicBlock ParseMagicBlock(WzooBlock block)
        {
            var magicBlock = new MagicBlock();

            if (BlockMagic.Length != 4)
            {
                throw new Exception(
                    $"the block magic was expected to be 4 bytes in length, found a length of {BlockMagic.Length}");
            }

            magicBlock.BlockMagic = BlockMagic;

            using (var ms = new MemoryStream(block.BlockData))
            {
                var emptyBuffer = new byte[4];
                var empty = new byte[] {0xDE, 0xAD, 0xBE, 0xEF};

                ms.Seek(-4, SeekOrigin.End);
                
                ms.Read(emptyBuffer, 0, 4);

                if (!emptyBuffer.SequenceEqual(empty))
                {
                    // Block without magic
                    magicBlock.IsMagicBlock = false;
                    ms.Seek(32, SeekOrigin.Begin);

                    var blockDataLength = block.BlockData.Length - 32;

                    magicBlock.BlockData = new byte[blockDataLength];
                    ms.Read(magicBlock.BlockData, 0, blockDataLength);
                }
                else
                {
                    ms.Seek(36, SeekOrigin.Begin);

                    var pluginNameLengthBuffer = new byte[4];
                    ms.Read(pluginNameLengthBuffer, 0, 4);
                    var pluginNameLength = BigEndian.ToInt32(pluginNameLengthBuffer, 0);

                    magicBlock.PluginName = new byte[pluginNameLength];

                    ms.Read(magicBlock.PluginName, 0, pluginNameLength);

                    var blockDataLength = block.BlockData.Length - 4 - pluginNameLength - 36;

                    magicBlock.BlockData = new byte[blockDataLength];
                    ms.Read(magicBlock.BlockData, 0, blockDataLength);
                }
            }

            return magicBlock;
        }

        /// <summary>
        /// Is called after the main parse process.
        /// </summary>
        public virtual void PostProcess()
        {
        }

        public byte[] GetParameters()
        {
            using (var ms = new MemoryStream())
            {
                // parameter count
                ms.Write(LittleEndian.GetBytes(Parameters.Count), 0, 4);

                // parameters
                foreach (var paramValue in Parameters)
                {
                    ms.Write(LittleEndian.GetBytes((float) paramValue), 0, 4);
                }

                return ms.ToByteArray();
            }
        }

        public virtual byte[] GetBlockDataToWrite()
        {
            return WzooBlock.GetDataToWrite();
        }
    }
}