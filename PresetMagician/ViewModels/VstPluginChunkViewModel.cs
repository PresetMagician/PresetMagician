using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Models;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.ViewModels
{
    public class VstPluginChunkViewModel : VstPluginViewModel
    {
        public VstPluginChunkViewModel(Plugin plugin, IVstService vstService, IOpenFileService openFileService,
            ISelectDirectoryService selectDirectoryService, ILicenseService licenseService) : base(plugin, vstService, openFileService,
            selectDirectoryService, licenseService)
        {
            Plugin = plugin;
            OpenWithHxDBank = new TaskCommand(OnOpenWithHxDBankExecute);
            OpenWithHxDPreset = new TaskCommand(OnOpenWithHxDPresetExecute);
            LoadBankChunk = new TaskCommand(OnLoadBankChunkExecute);
        }

        public TaskCommand LoadBankChunk { get; set; }

        private async Task OnLoadBankChunkExecute ()
        {
            try
            {
                _openFileService.Filter = "Binary Files (*.*)|*.*";
                _openFileService.IsMultiSelect = false;

                if (await _openFileService.DetermineFileAsync())
                {
                    Plugin.SetPresetChunk(File.ReadAllBytes(_openFileService.FileName), false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }
        
        
        public TaskCommand OpenWithHxDBank { get; set; }
        public TaskCommand OpenWithHxDPreset { get; set; }

        private async Task OnOpenWithHxDBankExecute()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, Plugin.ChunkBankMemoryStream.ToArray());

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

        private async Task OnOpenWithHxDPresetExecute()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, Plugin.ChunkPresetMemoryStream.ToArray());

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
    }
}