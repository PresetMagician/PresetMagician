using Catel.MVVM;
using PresetMagician.Core.Models;

namespace PresetMagician.ViewModels
{
    public class PresetViewModel : ViewModelBase
    {
        public PresetViewModel(Preset preset)
        {
            Preset = preset;
        }

        [Model] public Preset Preset { get; private set; }
    }
}