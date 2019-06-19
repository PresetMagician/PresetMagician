using System;
using System.IO;
using System.Threading.Tasks;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Drachenkatze.PresetMagician.Utils;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

namespace PresetMagician.ViewModels
{
    public class VstPluginChunkViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly IOpenFileService _openFileService;
        private readonly ISaveFileService _saveFileService;
        private readonly DeveloperService _developerService;

        public VstPluginChunkViewModel(IRemotePluginInstance pluginInstance, IOpenFileService openFileService,
            ISaveFileService saveFileService, DeveloperService developerService)
        {
            _openFileService = openFileService;
            _saveFileService = saveFileService;
            _developerService = developerService;
            Plugin = pluginInstance.Plugin;
            PluginInstance = pluginInstance;
            Title = "Plugin Info for " + Plugin.PluginName;

            OpenBankWithHexEditor = new TaskCommand(OnOpenBankWithHexEditorExecute);
            OpenPresetWithHexEditor = new TaskCommand(OnOpenPresetWithHexEditorExecute);
            SaveBankChunk = new TaskCommand(OnSaveBankChunkExecute);
            LoadBankChunk = new TaskCommand(OnLoadBankChunkExecute);
            Refresh = new Command(OnRefreshExecute);
        }

        public Plugin Plugin { get; protected set; }
        public event EventHandler BankChunkChanged;
        public event EventHandler PresetChunkChanged;

        public IRemotePluginInstance PluginInstance { get; }

        public TaskCommand LoadBankChunk { get; }
        public TaskCommand SaveBankChunk { get; }
        public Command Refresh { get; }

        public MemoryStream ChunkPresetMemoryStream { get; } = new MemoryStream();
        public MemoryStream ChunkBankMemoryStream { get; } = new MemoryStream();
        public string ChunkPresetHash { get; private set; }
        public string ChunkBankHash { get; private set; }

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

        private void OnRefreshExecute()
        {
            RefreshChunks();
        }

        public void RefreshChunks()
        {
            var bankChunk = PluginInstance.GetChunk(false);
            if (!(bankChunk is null))
            {
                ChunkBankMemoryStream.SetLength(0);
                ChunkBankMemoryStream.Write(bankChunk, 0, bankChunk.Length);

                ChunkBankHash = HashUtils.getIxxHash(bankChunk);
                BankChunkChanged?.Invoke(this, EventArgs.Empty);
            }

            var presetChunk = PluginInstance.GetChunk(true);
            if (!(presetChunk is null))
            {
                ChunkPresetMemoryStream.SetLength(0);
                ChunkPresetMemoryStream.Write(presetChunk, 0, presetChunk.Length);

                ChunkPresetHash = HashUtils.getIxxHash(presetChunk);

                PresetChunkChanged?.Invoke(this, EventArgs.Empty);
            }
        }


        private async Task OnSaveBankChunkExecute()
        {
            try
            {
                _saveFileService.Filter = "Binary Files (*.*)|*.*";


                if (await _saveFileService.DetermineFileAsync())
                {
                    File.WriteAllBytes(_saveFileService.FileName, ChunkBankMemoryStream.ToArray());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }


        public TaskCommand OpenBankWithHexEditor { get; }


        private async Task OnOpenBankWithHexEditorExecute()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, ChunkBankMemoryStream.ToArray());

            _developerService.StartHexEditor(tempFile);
        }

        public TaskCommand OpenPresetWithHexEditor { get; }

        private async Task OnOpenPresetWithHexEditorExecute()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, ChunkPresetMemoryStream.ToArray());

            _developerService.StartHexEditor(tempFile);
        }
    }
}