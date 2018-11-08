using System;
using System.IO;
using System.Text;

namespace CommonUtils
{
    /// <summary>
    /// Class for reading and writing binary files.
    /// Per Ivar Nerseth, 2011 - 2015
    /// perivar@nerseth.com
    /// </summary>
    public class BinaryFile
    {

        public enum ByteOrder : int
        {
            LittleEndian,
            BigEndian
        }

        private ByteOrder byteOrder = ByteOrder.LittleEndian;

        private BinaryReader binaryReader = null;
        private BinaryWriter binaryWriter = null;
        private FileStream fs = null;
        private MemoryStream memStream = null;

        #region Constructors
        public BinaryFile(string filePath) : this(filePath, ByteOrder.LittleEndian, false) { }

        public BinaryFile(string filePath, ByteOrder byteOrder) : this(filePath, byteOrder, false) { }

        public BinaryFile(string filePath, ByteOrder byteOrder, bool createFile)
        {
            if (createFile)
            {
                this.fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            }
            else
            {
                this.fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
            }

            this.binaryWriter = new BinaryWriter(fs, Encoding.Default);
            this.binaryReader = new BinaryReader(fs, Encoding.Default);
            this.byteOrder = byteOrder;
        }

        public BinaryFile(byte[] byteArray) : this(byteArray, ByteOrder.LittleEndian) { }

        public BinaryFile(byte[] byteArray, ByteOrder byteOrder)
        {
            this.memStream = new MemoryStream(byteArray);
            this.binaryReader = new BinaryReader(memStream);

            // Set position to the beginning of the stream.
            this.memStream.Position = 0;

            this.byteOrder = byteOrder;
        }

        public BinaryFile(MemoryStream stream, ByteOrder byteOrder)
        {
            this.memStream = stream;
            this.binaryWriter = new BinaryWriter(memStream, Encoding.Default);

            // Set position to the beginning of the stream.
            this.memStream.Position = 0;

            this.byteOrder = byteOrder;
        }
        #endregion

        #region Set and Get Position
        public long GetPosition()
        {
            long position = 0;
            if (null != fs)
            {
                position = fs.Position;
            }
            else if (null != memStream)
            {
                position = memStream.Position;
            }
            return position;
        }

        public void SetPosition(long position)
        {
            if (null != fs)
            {
                fs.Position = position;
            }
            else if (null != memStream)
            {
                memStream.Position = position;
            }
        }
        #endregion

        #region Read Methods
        public byte ReadByte()
        {
            return binaryReader.ReadByte();
        }

        public byte[] ReadBytes(int size)
        {
            return ReadBytes(0, size);
        }

        public byte[] ReadBytes(int offset, int size)
        {
            if (size <= 0) return new byte[0];

            var bytes = new byte[size];
            if (byteOrder == ByteOrder.LittleEndian)
            {
                int numBytesRead = binaryReader.Read(bytes, offset, size);
                return bytes;
            }
            else
            {
                int numBytesRead = binaryReader.Read(bytes, offset, size);
                Array.Reverse(bytes);
                return bytes;
            }
        }

        public byte[] ReadBytes(int offset, int size, ByteOrder byteOrder)
        {
            if (size <= 0) return new byte[0];

            var bytes = new byte[size];
            if (byteOrder == ByteOrder.LittleEndian)
            {
                int numBytesRead = binaryReader.Read(bytes, offset, size);
                return bytes;
            }
            else
            {
                int numBytesRead = binaryReader.Read(bytes, offset, size);
                Array.Reverse(bytes);
                return bytes;
            }
        }

        /// <summary>
        /// Read boolean (1 byte)
        /// </summary>
        /// <returns>boolean byte value</returns>
        public bool ReadBoolean()
        {
            byte booleanByte = ReadByte();
            return Convert.ToBoolean(booleanByte);
        }

        public UInt16 ReadUInt16()
        {
            return ReadUInt16(byteOrder);
        }

        public UInt16 ReadUInt16(ByteOrder byteOrder)
        {
            return ReadUInt16(binaryReader, byteOrder);
        }

        public UInt32 ReadUInt32()
        {
            return ReadUInt32(byteOrder);
        }

        public UInt32 ReadUInt32(ByteOrder byteOrder)
        {
            return ReadUInt32(binaryReader, byteOrder);
        }

        public UInt64 ReadUInt64()
        {
            return ReadUInt64(byteOrder);
        }

        public UInt64 ReadUInt64(ByteOrder byteOrder)
        {
            return ReadUInt64(binaryReader, byteOrder);
        }

        public short ReadInt16()
        {
            return ReadInt16(byteOrder);
        }

        public short ReadInt16(ByteOrder byteOrder)
        {
            return ReadInt16(binaryReader, byteOrder);
        }

        public int ReadInt32()
        {
            return ReadInt32(byteOrder);
        }

        public int ReadInt32(ByteOrder byteOrder)
        {
            return ReadInt32(binaryReader, byteOrder);
        }

        public long ReadInt64()
        {
            return ReadInt64(byteOrder);
        }

        public long ReadInt64(ByteOrder byteOrder)
        {
            return ReadInt64(binaryReader, byteOrder);
        }

        public float ReadSingle()
        {
            return ReadSingle(byteOrder);
        }

        public float ReadSingle(ByteOrder byteOrder)
        {
            return ReadSingle(binaryReader, byteOrder);
        }

        public double ReadDouble()
        {
            return ReadDouble(byteOrder);
        }

        public double ReadDouble(ByteOrder byteOrder)
        {
            return ReadDouble(binaryReader, byteOrder);
        }

        public char[] ReadChars(int size)
        {
            return binaryReader.ReadChars(size);
        }

        public string ReadString(int size)
        {
            return new string(binaryReader.ReadChars(size));
        }
        #endregion

        #region Seek Methods
        public long Seek(long offset)
        {
            return SeekTo(offset);
        }

        public long Seek(long offset, SeekOrigin seekOrigin)
        {
            return binaryReader.BaseStream.Seek(offset, seekOrigin);
        }

        public long SeekTo(long offset)
        {
            return binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
        }
        #endregion

        #region Write Methods
        public bool Write(short value)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                binaryWriter.Write(value);
            }
            else // Big-Endian
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                binaryWriter.Write(bytes);
            }
            return true;
        }

        public bool Write(int value)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                binaryWriter.Write(value);
            }
            else // Big-Endian
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                binaryWriter.Write(bytes);
            }
            return true;
        }

        public bool Write(long value)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                binaryWriter.Write(value);
            }
            else // Big-Endian
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                binaryWriter.Write(bytes);
            }
            return true;
        }

        public bool Write(ushort value)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                binaryWriter.Write(value);
            }
            else // Big-Endian
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                binaryWriter.Write(bytes);
            }
            return true;
        }

        public bool Write(uint value)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                binaryWriter.Write(value);
            }
            else // Big-Endian
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                binaryWriter.Write(bytes);
            }
            return true;
        }

        public bool Write(ulong value)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                binaryWriter.Write(value);
            }
            else // Big-Endian
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                binaryWriter.Write(bytes);
            }
            return true;
        }

        public bool Write(float value)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                binaryWriter.Write(value);
            }
            else // Big-Endian
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                binaryWriter.Write(bytes);
            }
            return true;
        }

        public bool Write(double value)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                binaryWriter.Write(value);
            }
            else // Big-Endian
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                binaryWriter.Write(bytes);
            }
            return true;
        }

        public bool Write(string value, int length)
        {
            binaryWriter.Write(StringToByteArray(value, length));
            return true;
        }

        public bool Write(string value)
        {
            binaryWriter.Write(StringToByteArray(value, value.Length));
            return true;
        }

        public bool Write(byte value)
        {
            binaryWriter.Write(value);
            return true;
        }

        public bool Write(byte[] value)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                binaryWriter.Write(value);
            }
            else // Big-Endian
            {
                Array.Reverse(value);
                binaryWriter.Write(value);
            }
            return true;
        }

        public bool Write(byte[] value, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                binaryWriter.Write(value);
            }
            else // Big-Endian
            {
                Array.Reverse(value);
                binaryWriter.Write(value);
            }
            return true;
        }

        public bool Write(char value)
        {
            binaryWriter.Write(value);
            return true;
        }

        public bool Write(char[] value)
        {
            binaryWriter.Write(value);
            return true;
        }

        public bool Write(bool value)
        {
            byte b = Convert.ToByte(value);
            binaryWriter.Write(b);
            return true;
        }
        #endregion

        #region Close Method
        public bool Close()
        {
            binaryReader.Close();
            binaryWriter.Close();
            fs.Close();
            return true;
        }
        #endregion

        #region Public Static Read Methods

        public static byte[] ReadBytes(BinaryReader reader, int fieldSize, ByteOrder byteOrder)
        {
            var bytes = new byte[fieldSize];
            if (byteOrder == ByteOrder.LittleEndian)
            {
                return reader.ReadBytes(fieldSize);
            }
            else
            {
                for (int i = fieldSize - 1; i > -1; i--)
                    bytes[i] = reader.ReadByte();
                return bytes;
            }
        }

        public static short ReadInt16(BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                return reader.ReadInt16();
            }
            else // Big-Endian
            {
                return BitConverter.ToInt16(ReadBytes(reader, 2, ByteOrder.BigEndian), 0);
            }
        }

        public static int ReadInt32(BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                return reader.ReadInt32();
            }
            else // Big-Endian
            {
                return BitConverter.ToInt32(ReadBytes(reader, 4, ByteOrder.BigEndian), 0);
            }
        }

        public static long ReadInt64(BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                return reader.ReadInt64();
            }
            else // Big-Endian
            {
                return BitConverter.ToInt64(ReadBytes(reader, 8, ByteOrder.BigEndian), 0);
            }
        }

        public static UInt16 ReadUInt16(BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                return reader.ReadUInt16();
            }
            else // Big-Endian
            {
                return BitConverter.ToUInt16(ReadBytes(reader, 2, ByteOrder.BigEndian), 0);
            }
        }

        public static UInt32 ReadUInt32(BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                return reader.ReadUInt32();
            }
            else // Big-Endian
            {
                return BitConverter.ToUInt32(ReadBytes(reader, 4, ByteOrder.BigEndian), 0);
            }
        }

        public static UInt64 ReadUInt64(BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                return reader.ReadUInt64();
            }
            else // Big-Endian
            {
                return BitConverter.ToUInt64(ReadBytes(reader, 8, ByteOrder.BigEndian), 0);
            }
        }

        public static float ReadSingle(BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                return reader.ReadSingle();
            }
            else // Big-Endian
            {
                return BitConverter.ToSingle(ReadBytes(reader, 4, ByteOrder.BigEndian), 0);
            }
        }

        public static double ReadDouble(BinaryReader reader, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.LittleEndian)
            {
                return reader.ReadDouble();
            }
            else // Big-Endian
            {
                return BitConverter.ToDouble(ReadBytes(reader, 8, ByteOrder.BigEndian), 0);
            }
        }
        #endregion

        #region Public Static Utility Methods

        /// <summary>
        /// Convert an uint into a single (float)
        /// </summary>
        /// <param name="x">uint</param>
        /// <param name="byteOrder">ByteOrder, Either LittleEndian or BigEndian</param>
        /// <returns>a single (float)</returns>
        public static float UIntToSingle(uint x, ByteOrder byteOrder)
        {
            return ByteArrayToSingle(BitConverter.GetBytes(x), byteOrder);
        }

        /// <summary>
        /// Convert a single (float) into an uint32
        /// </summary>
        /// <param name="x">single (float)</param>
        /// <param name="byteOrder">ByteOrder, Either LittleEndian or BigEndian</param>
        /// <returns>an uint32</returns>
        public static uint SingleToUInt(float x, ByteOrder byteOrder)
        {
            return ByteArrayToUInt32(BitConverter.GetBytes(x), byteOrder);
        }

        /// <summary>
        /// Convert a hex string to a byte array
        /// </summary>
        /// <param name="s">hex string</param>
        /// <returns>byte array</returns>
        public static byte[] HexStringToByteArray(string s)
        {
            var r = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
            {
                r[i / 2] = (byte)Convert.ToInt32(s.Substring(i, 2), 16);
            }
            return r;
        }

        /// <summary>
        /// Convert a byte array into a single (float)
        /// </summary>
        /// <param name="b">byte array</param>
        /// <param name="byteOrder">ByteOrder, Either LittleEndian or BigEndian</param>
        /// <returns>single (float)</returns>
        public static float ByteArrayToSingle(byte[] b, ByteOrder byteOrder)
        {
            if (b == null || b.Length < 4) return -1;

            if (byteOrder == ByteOrder.LittleEndian)
            {
                return BitConverter.ToSingle(b, 0);
            }
            else // Big-Endian
            {
                var bClone = (byte[])b.Clone();
                Array.Reverse(bClone);
                return BitConverter.ToSingle(bClone, 0);
            }
        }

        /// <summary>
        /// Convert a byte array into a double
        /// </summary>
        /// <param name="b">byte array</param>
        /// <param name="byteOrder">ByteOrder, Either LittleEndian or BigEndian</param>
        /// <returns>double</returns>
        public static double ByteArrayToDouble(byte[] b, ByteOrder byteOrder)
        {
            if (b == null || b.Length < 8) return -1;

            if (byteOrder == ByteOrder.LittleEndian)
            {
                return BitConverter.ToDouble(b, 0);
            }
            else // Big-Endian
            {
                var bClone = (byte[])b.Clone();
                Array.Reverse(bClone);
                return BitConverter.ToDouble(bClone, 0);
            }
        }

        /// <summary>
        /// Convert a byte array into an Int16
        /// </summary>
        /// <param name="b">byte array</param>
        /// <param name="byteOrder">ByteOrder, Either LittleEndian or BigEndian</param>
        /// <returns>Int16</returns>
        public static Int16 ByteArrayToInt16(byte[] b, ByteOrder byteOrder)
        {
            if (b == null || b.Length < 2) return -1;

            if (byteOrder == ByteOrder.LittleEndian)
            {
                return BitConverter.ToInt16(b, 0);
            }
            else // Big-Endian
            {
                var bClone = (byte[])b.Clone();
                Array.Reverse(bClone);
                return BitConverter.ToInt16(bClone, 0);
            }
        }

        /// <summary>
        /// Convert a byte array into an Int32
        /// </summary>
        /// <param name="b">byte array</param>
        /// <param name="byteOrder">ByteOrder, Either LittleEndian or BigEndian</param>
        /// <returns>Int32</returns>
        public static Int32 ByteArrayToInt32(byte[] b, ByteOrder byteOrder)
        {
            if (b == null || b.Length < 4) return -1;

            if (byteOrder == ByteOrder.LittleEndian)
            {
                return BitConverter.ToInt32(b, 0);
            }
            else // Big-Endian
            {
                var bClone = (byte[])b.Clone();
                Array.Reverse(bClone);
                return BitConverter.ToInt32(bClone, 0);
            }
        }

        /// <summary>
        /// Convert a byte array into an Int64
        /// </summary>
        /// <param name="b">byte array</param>
        /// <param name="byteOrder">ByteOrder, Either LittleEndian or BigEndian</param>
        /// <returns>Int64</returns>
        public static Int64 ByteArrayToInt64(byte[] b, ByteOrder byteOrder)
        {
            if (b == null || b.Length < 8) return -1;

            if (byteOrder == ByteOrder.LittleEndian)
            {
                return BitConverter.ToInt64(b, 0);
            }
            else // Big-Endian
            {
                var bClone = (byte[])b.Clone();
                Array.Reverse(bClone);
                return BitConverter.ToInt64(bClone, 0);
            }
        }

        /// <summary>
        /// Convert a byte array into an UInt16
        /// </summary>
        /// <param name="b">byte array</param>
        /// <param name="byteOrder">ByteOrder, Either LittleEndian or BigEndian</param>
        /// <returns>UInt16</returns>
        public static UInt16 ByteArrayToUInt16(byte[] b, ByteOrder byteOrder)
        {
            if (b == null || b.Length < 2) return 0;

            if (byteOrder == ByteOrder.LittleEndian)
            {
                return BitConverter.ToUInt16(b, 0);
            }
            else // Big-Endian
            {
                var bClone = (byte[])b.Clone();
                Array.Reverse(bClone);
                return BitConverter.ToUInt16(bClone, 0);
            }
        }

        /// <summary>
        /// Convert a byte array into an UInt32
        /// </summary>
        /// <param name="b">byte array</param>
        /// <param name="byteOrder">ByteOrder, Either LittleEndian or BigEndian</param>
        /// <returns>UInt32</returns>
        public static UInt32 ByteArrayToUInt32(byte[] b, ByteOrder byteOrder)
        {
            if (b == null || b.Length < 4) return 0;

            if (byteOrder == ByteOrder.LittleEndian)
            {
                return BitConverter.ToUInt32(b, 0);
            }
            else // Big-Endian
            {
                var bClone = (byte[])b.Clone();
                Array.Reverse(bClone);
                return BitConverter.ToUInt32(bClone, 0);
            }
        }

        /// <summary>
        /// Convert a byte array into an UInt64
        /// </summary>
        /// <param name="b">byte array</param>
        /// <param name="byteOrder">ByteOrder, Either LittleEndian or BigEndian</param>
        /// <returns>UInt64</returns>
        public static UInt64 ByteArrayToUInt64(byte[] b, ByteOrder byteOrder)
        {
            if (b == null || b.Length < 8) return 0;

            if (byteOrder == ByteOrder.LittleEndian)
            {
                return BitConverter.ToUInt64(b, 0);
            }
            else // Big-Endian
            {
                var bClone = (byte[])b.Clone();
                Array.Reverse(bClone);
                return BitConverter.ToUInt64(bClone, 0);
            }
        }

        /// <summary>
        /// Convert a hex string into an int
        /// </summary>
        /// <param name="hexString">hex string</param>
        /// <returns>int</returns>
        public static int HexToInt(string hexString)
        {
            return Convert.ToInt32(hexString, 16);
        }

        /// <summary>
        /// Convert a char array to string using the default encoding
        /// </summary>
        /// <param name="charArray">array of chars</param>
        /// <returns>a string</returns>
        public static byte[] CharArrayToByteArray(char[] charArray)
        {
            return Encoding.Default.GetBytes(charArray);
        }

        /// <summary>
        /// Convert a string to a byte array of a given length
        /// </summary>
        /// <param name="str">string to convert</param>
        /// <param name="length">length</param>
        /// <returns>a byte array</returns>
        public static byte[] StringToByteArray(string str, int length)
        {
            char[] charArray = str.ToCharArray();
            var byteArray = new byte[length];
            for (int i = 0; i < length; i++)
            {
                if (i < charArray.Length) byteArray[i] = Convert.ToByte(charArray[i]);
            }
            return byteArray;
        }

        /// <summary>
        /// Convert a string into an array of bytes
        /// </summary>
        /// <param name="str">string to convert</param>
        /// <returns>an array of bytes</returns>
        public static byte[] StringToByteArray(string str)
        {
            return Encoding.Default.GetBytes(str);
        }

        /// <summary>
        /// Return a 4 character string as an Int32
        /// </summary>
        /// <param name="str">4 character string</param>
        /// <param name="byteOrder">ByteOrder, Either LittleEndian or BigEndian</param>
        /// <returns>Int32</returns>
        public static Int32 StringToInt32(string str, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            if (str.Length > 4)
            {
                throw new ArgumentException("Passed string has more than 4 characters.", str);
            }
            return ByteArrayToInt32(StringToByteArray(str), byteOrder);
        }

        /// <summary>
        /// Convert a byte array to string using the default encoding
        /// </summary>
        /// <param name="byteArray">array of bytes</param>
        /// <returns>a string</returns>
        public static string ByteArrayToString(byte[] byteArray)
        {
            return Encoding.Default.GetString(byteArray);
        }

        /// <summary>
        /// Append to byte arrays
        /// </summary>
        /// <param name="a">first byte array</param>
        /// <param name="b">second byte array</param>
        /// <returns>the appended byte arrays</returns>
        public static byte[] AppendArrays(byte[] a, byte[] b)
        {
            var c = new byte[a.Length + b.Length]; // just one array allocation
            Buffer.BlockCopy(a, 0, c, 0, a.Length);
            Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }

        /// <summary>
        /// Function to save byte array to a file
        /// </summary>
        /// <param name="fileName">File name to save byte array</param>
        /// <param name="byteArray">Byte array to save to external file</param>
        /// <returns>Return true if byte array save successfully, if not return false</returns>
        public static bool Write(string fileName, byte[] byteArray)
        {
            return ByteArrayToFile(fileName, byteArray);
        }

        /// <summary>
        /// Function to save byte array to a file
        /// </summary>
        /// <param name="fileName">File name to save byte array</param>
        /// <param name="byteArray">Byte array to save to external file</param>
        /// <returns>Return true if byte array save successfully, if not return false</returns>
        public static bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                File.WriteAllBytes(fileName, byteArray);
                return true;
            }
            catch (Exception _Exception)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
            }

            // error occured, return false
            return false;
        }

        /// <summary>
        /// Convert a MemoryStream into a float array
        /// </summary>
        /// <param name="stream">a MemoryStream</param>
        /// <returns>float array</returns>
        public static float[] FloatArrayFromStream(System.IO.MemoryStream stream)
        {
            return FloatArrayFromByteArray(stream.GetBuffer());
        }

        /// <summary>
        /// Convert a byte array into a float array
        /// </summary>
        /// <param name="byteArray">array of bytes</param>
        /// <returns>float array</returns>
        public static float[] FloatArrayFromByteArray(byte[] byteArray)
        {
            var floatArray = new float[byteArray.Length / 4];
            Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
            return floatArray;
        }

        /// <summary>
        /// Convert a Float Array into a Byte Array
        /// Note that this method does not do Bit conversion!
        /// E.g. converting from e.g. IEEE 32 bit float to 16 bit pcm bytes
        /// If you want that you can you the Accord.Audio.SampleConverter.Convert method to
        /// converts an array of 32-bit float (IEEE ) samples into a array of 16-bit short samples,
        /// and then ByteArrayFromShortArray(short[] pcm16Samples) to convert it into bytes.
        /// </summary>
        /// <param name="floatArray">array of floats</param>
        /// <returns>byte array</returns>
        public static byte[] ByteArrayFromFloatArray(float[] floatArray)
        {

            // create a byte array and copy the 32 bit floats into it...
            var byteArray = new byte[floatArray.Length * 4];
            Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }

        /// <summary>
        /// Convert a float array into a MemoryStream
        /// Note that this method does not do Bit conversion!
        /// E.g. converting from e.g. IEEE 32 bit float to 16 bit pcm bytes
        /// If you want that you can you the Accord.Audio.SampleConverter.Convert method to
        /// converts an array of 32-bit float (IEEE ) samples into a array of 16-bit short samples,
        /// and then ByteArrayFromShortArray(short[] pcm16Samples) to convert it into bytes.
        /// </summary>
        /// <param name="floatArray">array of floats</param>
        /// <returns>a MemoryStream</returns>
        public static MemoryStream FloatArrayToMemoryStream(float[] floatArray)
        {
            var byteArray = ByteArrayFromFloatArray(floatArray);
            return new MemoryStream(byteArray);
        }

        /// <summary>
        /// Convert a Short Array into a Byte Array
        /// Note that this method does not do Bit conversion!
        /// E.g. converting from e.g. IEEE 32 bit float to 16 bit pcm bytes
        /// If you want that you can you the Accord.Audio.SampleConverter.Convert method to
        /// converts an array of 32-bit float (IEEE ) samples into a array of 16-bit short samples,
        /// and then ByteArrayFromShortArray(short[] pcm16Samples) to convert it into bytes.
        /// </summary>
        /// <param name="shortArray">array of 16 bit shorts</param>
        /// <returns>byte array</returns>
        /// <example>
        /// var pcm16Samples = new short[waveformData.Length];
        /// Accord.Audio.SampleConverter.Convert(waveformData, pcm16Samples);
        /// var byteArray = CommonUtils.BinaryFile.ByteArrayFromShortArray(pcm16Samples);
        /// </example>
        public static byte[] ByteArrayFromShortArray(short[] shortArray)
        {

            // create a byte array and copy the 16 bit shorts into it...
            var byteArray = new byte[shortArray.Length * 2];
            Buffer.BlockCopy(shortArray, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }

        /// <summary>
        /// Convert a float array into a MemoryStream
        /// Note that this method does not do Bit conversion!
        /// E.g. converting from e.g. IEEE 32 bit float to 16 bit pcm bytes
        /// If you want that you can you the Accord.Audio.SampleConverter.Convert method to
        /// converts an array of 32-bit float (IEEE ) samples into a array of 16-bit short samples,
        /// and then ByteArrayFromShortArray(short[] pcm16Samples) to convert it into bytes.
        /// </summary>
        /// <param name="shortArray">array of 16 bit shorts</param>
        /// <returns>a MemoryStream</returns>
        public static MemoryStream ShortArrayToMemoryStream(short[] shortArray)
        {
            var byteArray = ByteArrayFromShortArray(shortArray);
            return new MemoryStream(byteArray);
        }
        #endregion

        public override string ToString()
        {
            if (this.fs != null)
            {
                return string.Format("Filestream: {1}/{0}", fs.Length, fs.Position);
            }
            else if (this.memStream != null)
            {
                return string.Format("Memstream: {1}/{0}", memStream.Length, memStream.Position);
            }
            else
            {
                return string.Format("BinaryFile");
            }
        }

    }
}