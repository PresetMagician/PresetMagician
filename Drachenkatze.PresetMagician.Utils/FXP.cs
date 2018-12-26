using System;
using System.IO;

namespace Drachenkatze.PresetMagician.Utils
{
    /// <summary>
    /// Class for reading and writing Steinberg Preset files and Bank files (fxp and fxb files).
    /// Per Ivar Nerseth, 2011 - 2012
    /// perivar@nerseth.com
    /// </summary>
    public class FXP
    {
        /*
		// Preset (Program) (.fxp) with chunk (magic = 'FPCh')
		typedef struct
		{
			char chunkMagic[4];     // 'CcnK'
			long byteSize;          // of this chunk, excl. magic + byteSize
			char fxMagic[4];        // 'FPCh'
			long version;
			char fxID[4];           // fx unique id
			long fxVersion;
			long numPrograms;
			char name[28];
			long chunkSize;
			unsigned char chunkData[chunkSize];
		} fxProgramSet;

		// Bank (.fxb) with chunk (magic = 'FBCh')
		typedef struct
		{
			char chunkMagic[4];     // 'CcnK'
			long byteSize;          // of this chunk, excl. magic + byteSize
			char fxMagic[4];        // 'FBCh'
			long version;
			char fxID[4];           // fx unique id
			long fxVersion;
			long numPrograms;
			char future[128];
			long chunkSize;
			unsigned char chunkData[chunkSize];
		} fxChunkSet;

		// For Preset (Program) (.fxp) without chunk (magic = 'FxCk')
		typedef struct {
    		char chunkMagic[4];     // 'CcnK'
    		long byteSize;          // of this chunk, excl. magic + byteSize
    		char fxMagic[4];        // 'FxCk'
    		long version;
    		char fxID[4];           // fx unique id
    		long fxVersion;
    		long numParams;
    		char prgName[28];
    		float params[numParams];        // variable no. of parameters
		} fxProgram;

		// For Bank (.fxb) without chunk (magic = 'FxBk')
		typedef struct {
    		char chunkMagic[4];     // 'CcnK'
    		long byteSize;          // of this chunk, excl. magic + byteSize
    		char fxMagic[4];        // 'FxBk'
    		long version;
    		char fxID[4];           // fx unique id
    		long fxVersion;
    		long numPrograms;
    		char future[128];
    		fxProgram programs[numPrograms];  // variable no. of programs
		} fxSet;
		 */

        private string chunkMagic;      // 'CcnK'
        private int byteSize;           // of this chunk, excl. magic + byteSize
        private string fxMagic;         // 'FxCk', 'FxBk', 'FBCh' or 'FPCh'
        private int version;
        private string fxID;
        private int fxVersion;
        private int numPrograms;        // FPCh = numProgams
        private int numParameters;      // FxCk = numParams
        private float[] parameters;     // FxCk = float[numParameters]
        private string name;            // if FPCh
        private string future;          // if FBCh
        private int chunkSize;

        private string chunkData;
        private byte[] chunkDataByteArray;

        public string ChunkMagic { get { return chunkMagic; } set { chunkMagic = value; } }
        public int ByteSize { get { return byteSize; } set { byteSize = value; } }
        public string FxMagic { get { return fxMagic; } set { fxMagic = value; } }
        public int Version { get { return version; } set { version = value; } }
        public string FxID { get { return fxID; } set { fxID = value; } }
        public int FxVersion { get { return fxVersion; } set { fxVersion = value; } }
        public int ProgramCount { get { return numPrograms; } set { numPrograms = value; } }
        public int ParameterCount { get { return numParameters; } set { numParameters = value; } }
        public float[] Parameters { get { return parameters; } set { parameters = value; } }
        public string Name { get { return name; } set { name = value; } }
        public string Future { get { return future; } set { future = value; } }
        public int ChunkSize { get { return chunkSize; } set { chunkSize = value; } }
        public string ChunkData { get { return chunkData; } set { chunkData = value; } }
        public byte[] ChunkDataByteArray { get { return chunkDataByteArray; } set { chunkDataByteArray = value; } }

        // default constructor
        public FXP()
        {
        }

        // constructor with byte array
        public FXP(byte[] values)
        {
            BinaryFile bf = new BinaryFile(values, BinaryFile.ByteOrder.BigEndian);
            ReadFXP(bf);
        }

        public void WriteFile(string filePath)
        {
            BinaryFile bf = new BinaryFile(filePath, BinaryFile.ByteOrder.BigEndian, true);

            if (ChunkMagic != "CcnK")
            {
                Console.Out.WriteLine("Cannot save the preset file. Missing preset header information.");
                return;
            }

            Console.Out.WriteLine("Writing FXP to {0} ...", filePath);

            bf.Write(ChunkMagic);                           // chunkMagic, 4

            // check what preset type we are saving
            if (FxMagic == "FBCh")
            {
                // Bank with Chunk Data
                ByteSize = 152 + ChunkSize;

                bf.Write(ByteSize);                         // byteSize = 4
                bf.Write(FxMagic);                          // fxMagic, 4
                bf.Write(Version);                          // version, 4
                bf.Write(FxID);                             // fxID, 4
                bf.Write(FxVersion);                        // fxVersion, 4
                bf.Write(ProgramCount);                     // numPrograms, 4
                bf.Write(Future, 128);                      // future, 128
                bf.Write(ChunkSize);                        // chunkSize, 4

              
                    // Even though the main FXP is BigEndian format the preset chunk is saved in LittleEndian format
                    bf.Write(chunkDataByteArray, BinaryFile.ByteOrder.LittleEndian);
            }
            else if (FxMagic == "FPCh")
            {
                // Preset with Chunk Data
                ByteSize = 52 + ChunkSize;

                bf.Write(ByteSize);                         // byteSize = 4
                bf.Write(FxMagic);                          // fxMagic, 4
                bf.Write(Version);                          // version, 4
                bf.Write(FxID);                             // fxID, 4
                bf.Write(FxVersion);                        // fxVersion, 4
                bf.Write(ProgramCount);                     // numPrograms, 4
                bf.Write(Name, 28);                         // name, 28
                bf.Write(ChunkSize);                        // chunkSize, 4

               
                    // Even though the main FXP is BigEndian format the preset chunk is saved in LittleEndian format
                    bf.Write(chunkDataByteArray, BinaryFile.ByteOrder.LittleEndian);
               
            }
            else if (FxMagic == "FxCk")
            {
                // For Preset (Program) (.fxp) without chunk (magic = 'FxCk')
                // Bank with Chunk Data
                ByteSize = 48 + (4 * ParameterCount);

                bf.Write(ByteSize);                         // byteSize = 4
                bf.Write(FxMagic);                          // fxMagic, 4
                bf.Write(Version);                          // version, 4
                bf.Write(FxID);                             // fxID, 4
                bf.Write(FxVersion);                        // fxVersion, 4
                bf.Write(ParameterCount);                   // numParameters, 4
                bf.Write(Name, 28);                         // name, 28

                // variable no. of parameters
                for (int i = 0; i < ParameterCount; i++)
                {
                    bf.Write((float)Parameters[i]);
                }
            }

            bf.Close();
        }

        public void ReadFile(string filePath)
        {
            BinaryFile bf = new BinaryFile(filePath, BinaryFile.ByteOrder.BigEndian);
            ReadFXP(bf);
        }

        public void ReadFXP(BinaryFile bf)
        {
            ChunkMagic = bf.ReadString(4);
            if (ChunkMagic != "CcnK")
            {
                Console.Out.WriteLine("Error reading file. Missing preset header information.");
            }

            ByteSize = bf.ReadInt32();
            FxMagic = bf.ReadString(4);

            if (FxMagic == "FBCh")
            {
                // Bank with Chunk Data
                Version = bf.ReadInt32();
                FxID = bf.ReadString(4);
                FxVersion = bf.ReadInt32();
                ProgramCount = bf.ReadInt32();
                Future = bf.ReadString(128);
                ChunkSize = bf.ReadInt32();

                // Even though the main FXP is BigEndian format the preset chunk is saved in LittleEndian format
                chunkDataByteArray = new byte[ChunkSize];
                chunkDataByteArray = bf.ReadBytes(0, ChunkSize, BinaryFile.ByteOrder.LittleEndian);
                chunkData = BinaryFile.ByteArrayToString(chunkDataByteArray);
            }
            else if (FxMagic == "FPCh")
            {
                // Preset with Chunk Data
                Version = bf.ReadInt32();
                FxID = bf.ReadString(4);
                FxVersion = bf.ReadInt32();
                ProgramCount = bf.ReadInt32();
                Name = bf.ReadString(28);
                ChunkSize = bf.ReadInt32();

                // Even though the main FXP is BigEndian format the preset chunk is saved in LittleEndian format
                chunkDataByteArray = new byte[ChunkSize];
                chunkDataByteArray = bf.ReadBytes(0, ChunkSize, BinaryFile.ByteOrder.LittleEndian);
                chunkData = BinaryFile.ByteArrayToString(chunkDataByteArray);
            }
            else if (FxMagic == "FxCk")
            {
                // For Preset (Program) (.fxp) without chunk (magic = 'FxCk')
                Version = bf.ReadInt32();
                FxID = bf.ReadString(4);
                FxVersion = bf.ReadInt32();
                ParameterCount = bf.ReadInt32();
                Name = bf.ReadString(28);

                // variable no. of parameters
                Parameters = new float[ParameterCount];
                for (int i = 0; i < ParameterCount; i++)
                {
                    Parameters[i] = bf.ReadSingle();
                }
            }

            bf.Close();
        }
    }
}