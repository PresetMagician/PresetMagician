using System;
using System.Diagnostics;
using Drachenkatze.PresetMagician.Utils;

namespace PresetMagicianScratchPad
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var fxp = new FXP();
            fxp.ReadFile(@"C:\Users\Drachenkatze\Documents\Xfer\Serum Presets\Presets\Bass\BA FM Bounce [ASL].fxp");

            Debug.WriteLine("FXP Name: " + fxp.Name.Trim());
            Debug.WriteLine("FXP chunk size: " + fxp.ChunkSize);

            Debug.Write(HexDump.GetHexDump(BinaryFile.StringToByteArray(fxp.Name.Trim('\0'))));
        }
    }
}