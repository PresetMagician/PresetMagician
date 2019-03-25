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
        
        public PresetDataViewModel(Preset preset, PresetDataPersisterService presetDataPersisterService) : base(preset)
        {
            _presetDataPersisterService = presetDataPersisterService;
            OpenWithHxD = new Command(OnOpenWithHxDExecute);
        }

        protected override async Task InitializeAsync()
        {
            PresetData = await _presetDataPersisterService.GetPresetData(Preset);
            
            await base.InitializeAsync();
        }
        
        public Command OpenWithHxD { get; set; }


        private void OnOpenWithHxDExecute()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, PresetData);

            var process = new Process
            {
                StartInfo =
                {
                    FileName = @"C:\Program Files\HxD\HxD.exe",
                    Arguments = tempFile
                }
            };

            process.Start();
        }

        public byte[] PresetData { get; private set; }
    }
}