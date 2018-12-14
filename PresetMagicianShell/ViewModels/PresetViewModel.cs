using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Fody;
using Catel.MVVM;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace PresetMagicianShell.ViewModels
{
    public class PresetViewModel: ViewModelBase
    {
        public PresetViewModel(Preset preset)
        {
            Preset = preset;
        }

        [Model]
        public Preset Preset { get; private set; }
    }
}
