using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Fody;
using Catel.MVVM;
using Drachenkatze.PresetMagician.VendorPresetParser;

namespace PresetMagician.ViewModels
{
    class PresetBankViewModel: ViewModelBase
    {
        public PresetBankViewModel(PresetBank presetBank)
        {
            PresetBank = presetBank;
        }

        [Model]
        [Expose("BankName")]
        public PresetBank PresetBank { get; private set; }
    }
}
