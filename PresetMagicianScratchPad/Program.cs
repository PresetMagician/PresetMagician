using System;
using System.Diagnostics;
using System.IO;
using PresetMagician.VendorPresetParser.RevealSound.Internal;

namespace PresetMagicianScratchPad
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            
            var bankFile = @"C:\Users\Drachenkatze\AppData\Roaming\RevealSound\Banks\EDM_Remastered_Vol1_Free.sbf";
            var s = new SpireBank();
            //s.ParseDiskFile(File.ReadAllBytes(@"C:\Users\Drachenkatze\Documents\SPIRE Micro SK.sbf"));
            s.ParseDiskFile(File.ReadAllBytes(bankFile));
            //s.ParseDiskFile(File.ReadAllBytes(@"C:\Program Files\VSTPlugins\Spire-1.1_x64\factory.sbf"));

            Debug.WriteLine(
                $"Bank Name: {s.BankName} Bank Name 2: {s.BankName2} Company Name: {s.CompanyName} Company URL: {s.CompanyUrl}");

            foreach (var p in s.Presets)
            {
                Debug.WriteLine($"Program {s.Presets.IndexOf(p)}: {p.ProgramName}");
            }

            var cfg = new SpireJsonConfig();
            cfg.SelectedBank = bankFile;

            for (var i = 0; i < s.Presets.Count; i++)
            {
                File.WriteAllBytes(@"C:\Users\Drachenkatze\Documents\spire-test.bin",
                    s.GenerateMemoryBank(s.Presets[0], cfg));
            }


        }
    }

    

  
  

   

   
}