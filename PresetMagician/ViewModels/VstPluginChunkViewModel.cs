using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using SharedModels;

namespace PresetMagician.ViewModels
{
    public class VstPluginChunkViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly IOpenFileService _openFileService;

        public VstPluginChunkViewModel(IRemotePluginInstance pluginInstance, IOpenFileService openFileService)
        {
            _openFileService = openFileService;
            Plugin = pluginInstance.Plugin;
            PluginInstance = pluginInstance;
            Title = "Plugin Info for " + Plugin.PluginName;

            OpenWithHxDBank = new TaskCommand(OnOpenWithHxDBankExecute);
            OpenWithHxDPreset = new TaskCommand(OnOpenWithHxDPresetExecute);
            LoadBankChunk = new TaskCommand(OnLoadBankChunkExecute);
        }

        public Plugin Plugin { get; protected set; }

        public IRemotePluginInstance PluginInstance { get; protected set; }

        public TaskCommand LoadBankChunk { get; set; }

        private async Task OnLoadBankChunkExecute()
        {
            try
            {
                _openFileService.Filter = "Binary Files (*.*)|*.*";
                _openFileService.IsMultiSelect = false;

                if (await _openFileService.DetermineFileAsync())
                {
                    PluginInstance.SetChunk(File.ReadAllBytes(_openFileService.FileName), false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }


        public TaskCommand OpenWithHxDBank { get; set; }


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

        public TaskCommand OpenWithHxDPreset { get; set; }

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