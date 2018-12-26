using System;
using System.Diagnostics;
using System.Net.Mime;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Windows;
using Drachenkatze.PresetMagician.Utils;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagician.Models;

namespace PresetMagicianScratchPad
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var fxp = new FXP();
            fxp.ReadFile(@"C:\Users\Drachenkatze\Documents\Xfer\Serum Presets\Presets\Bass\BA FM Bounce [ASL].fxp");

            Debug.WriteLine("FXP Name: "+fxp.Name.Trim());
            Debug.WriteLine("FXP chunk size: "+fxp.ChunkSize);

            Debug.Write(HexDump.GetHexDump(BinaryFile.StringToByteArray(fxp.Name.Trim('\0'))));
        }
    }
}