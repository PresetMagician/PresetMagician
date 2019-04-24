using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Catel.MVVM;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

namespace PresetMagician.ViewModels
{
    public class PresetDataViewModel : PresetViewModel
    {
        private PresetDataPersisterService _presetDataPersisterService;
        private readonly DeveloperService _developerService;
        
        public PresetDataViewModel(Preset preset, PresetDataPersisterService presetDataPersisterService, DeveloperService developerService) : base(preset)
        {
            _developerService = developerService;
            _presetDataPersisterService = presetDataPersisterService;
            OpenWithHexEditor = new Command(OnOpenWithHexEditor);
        }

        protected override async Task InitializeAsync()
        {
            PresetData = await _presetDataPersisterService.GetPresetData(Preset);
            
            await base.InitializeAsync();
        }
        
        public Command OpenWithHexEditor { get; set; }


        private void OnOpenWithHexEditor()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, PresetData);

            _developerService.StartHexEditor(tempFile);
           
        }

        public byte[] PresetData { get; private set; }
    }
}