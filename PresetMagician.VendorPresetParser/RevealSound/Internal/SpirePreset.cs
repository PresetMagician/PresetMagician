using System.Diagnostics;
using System.IO;
using System.Linq;
using GSF.IO;
using PresetMagician.Utils;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.RevealSound.Internal
{
    public class SpirePreset
    {
        private const int PRESET_NAME_LENGTH = 24;
        private const int STUFF_AFTER_PRESET_NAME_LENGTH = 253;
        private static readonly byte[] WORKAROUND_FILLER = {0x00, 0x00, 0x00, 0x00};
        public string ProgramName { get; private set; }
        public byte[] StuffAfterProgramName { get; private set; }
        public byte[] ProgramData { get; private set; }

        public int NumParameters { get; private set; }

        public SpirePreset(int numParameters = 0)
        {
            NumParameters = numParameters;
        }

        /// <summary>
        /// Creates a copy of the program and sets the name to "init".
        /// </summary>
        /// <returns></returns>
        public SpirePreset CreateBlank()
        {
            var sp = new SpirePreset {ProgramName = "init"};
            StuffAfterProgramName.CopyTo(sp.StuffAfterProgramName, 0);
            ProgramData.CopyTo(sp.ProgramData, 0);

            return sp;
        }


        public void FillMissingParametersFrom(SpirePreset preset)
        {
            for (var i = NumParameters; i < preset.NumParameters; i++)
            {
                SetParameter(i, preset.GetParameter(i));
            }

            NumParameters = preset.NumParameters;
        }

        public byte[] GetParameter(int index)
        {
            if (NumParameters < index)
            {
                throw new SpireException(
                    $"Tried to retrieve parameter number {index} but the preset only contains " +
                    $"{NumParameters} parameters");
            }

            var desiredLength = (index + 1) * 4;

            if (ProgramData.Length < desiredLength)
            {
                throw new SpireException(
                    $"Tried to retrieve parameter number {index} but the preset data length of {ProgramData.Length} " +
                    $"holds not enough parameters (requested parameter index {index}, desired length {desiredLength})");
            }

            using (var ms = new MemoryStream(ProgramData))
            {
                ms.Seek(index * 4, SeekOrigin.Begin);
                return ms.ReadBytes(4);
            }
        }

        public void SetParameter(int index, byte[] data)
        {
            if (data.Length != 4)
            {
                throw new SpireException(
                    $"The data length for a parameter must be 4 bytes, {data.Length} bytes given.");
            }

            var buf = ProgramData;
            var desiredLength = (index + 1) * 4;

            if (buf.Length < desiredLength)
            {
                buf = new byte[desiredLength];
                ProgramData.CopyTo(buf, 0);
                data.CopyTo(buf, index * 4);
            }

            ProgramData = buf;
        }

        public void Export(MemoryStream ms, bool programDataOnly = false)
        {
            if (!programDataOnly)
            {
                ms.WriteNullTerminatedString(ProgramName, PRESET_NAME_LENGTH);
                ms.Write(StuffAfterProgramName);
            }

            ms.Write(ProgramData);
        }

        public void ParseFromBank(MemoryStream ms, bool workaroundForLegacy)
        {
            ProgramName = ms.ReadNullTerminatedString(PRESET_NAME_LENGTH);
            StuffAfterProgramName = ms.ReadBytes(STUFF_AFTER_PRESET_NAME_LENGTH);

            var endPos = ms.Position + NumParameters * 4;
            if (endPos > ms.Length)
            {
                if (workaroundForLegacy)
                {
                    ProgramData = ms.ReadBytes((NumParameters - 1) * 4);
                    ProgramData = ProgramData.Concat(WORKAROUND_FILLER).ToArray();
                }
                else
                {
                    throw new SpireException("Unexpected end of file.");
                }
            }
            else
            {
                ProgramData = ms.ReadBytes(NumParameters * 4);
            }
        }

        public void ParseDiskFile(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var pd = new MemoryStream())
            {
                ProgramName = ms.ReadNullTerminatedString(PRESET_NAME_LENGTH);
                StuffAfterProgramName = ms.ReadBytes(STUFF_AFTER_PRESET_NAME_LENGTH);
                NumParameters = 0;

                while (ms.Position + 4 <= ms.Length)
                {
                    var param = ms.ReadBytes(4);
                    pd.Write(param, 0, 4);
                    NumParameters++;
                }

                ProgramData = pd.ToArray();
            }
        }
    }
}