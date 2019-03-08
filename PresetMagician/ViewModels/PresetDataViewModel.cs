using Catel.MVVM;
using PresetMagician.Services.Interfaces;
using SharedModels;
using SharedModels.Models;

namespace PresetMagician.ViewModels
{
    public class PresetDataViewModel : PresetViewModel
    {
        public PresetDataViewModel(Preset preset, IVstService vstService): base(preset)
        {
            PresetData = vstService.GetPresetData(preset);
        }
        
        public byte[] PresetData { get; }
    }
}