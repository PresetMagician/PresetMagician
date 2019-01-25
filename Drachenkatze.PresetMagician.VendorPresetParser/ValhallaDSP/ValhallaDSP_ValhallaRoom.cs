using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Drachenkatze.PresetMagician.VendorPresetParser.ValhallaDSP
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class ValhallaDSP_ValhallaRoom : ValhallaDSP, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> {1383429485};

        protected override string GetParseDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                @"Valhalla DSP, LLC\ValhallaRoom\Factory Presets");
        }

        public override void Init()
        {
            base.Init();
            BankLoadingNotes = $"Factory Presets are loaded from {GetParseDirectory()}. Sub-folders define the bank. User presets are " +
                               "currently not supported, if you know where user presets are saved please let me know";
            
            DefaultModes.Add("Reverb");
            DefaultModes.Add("Room");
            
            
        }
        
      
       
    }
}