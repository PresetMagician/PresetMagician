using System;
using Catel.MVVM;
using System.IO;
using Catel;
using Catel.Logging;
using Catel.Services;
using Orc.FileSystem;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Newtonsoft.Json;

namespace PresetMagicianShell.ViewModels
{
    public class NKSFViewModel : ViewModelBase
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IFileService _fileService;
        private readonly IOpenFileService _openFileService;
        private readonly IPleaseWaitService _pleaseWaitService;

        #endregion Fields

        public string SummaryInformation { get; private set; }
        public string ControllerAssignments { get; private set; }
        public string PluginId { get; private set; }
        public MemoryStream PluginChunk { get; set; }
        public string filePath { get; set; }

        public NKSFViewModel(IOpenFileService openFileService, IFileService fileService, IPleaseWaitService pleaseWaitService)
        {
            Argument.IsNotNull(() => openFileService);
            Argument.IsNotNull(() => fileService);
            Argument.IsNotNull(() => pleaseWaitService);

            _openFileService = openFileService;
            _fileService = fileService;
            _pleaseWaitService = pleaseWaitService;

            Title = "NKSF Viewer";

            OpenNKSFFile = new TaskCommand(OnOpenNKSFFileExecute);
        }

        #region Commands

        public TaskCommand OpenNKSFFile { get; private set; }

        private async Task OnOpenNKSFFileExecute()
        {
            try
            {
                _openFileService.Filter = "NKSF Files (*.nksf)|*.nksf";
                _openFileService.IsMultiSelect = false;

                if (await _openFileService.DetermineFileAsync())
                {
                    filePath = _openFileService.FileName;

                    OnParseNKSFFileExecute();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }

        private void OnParseNKSFFileExecute()
        {
            Log.Debug("Trying to parse NKSF");
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                NKSFRiff n = new NKSFRiff();
                n.Read(fileStream);

                SummaryInformation = FormatJson(n.kontaktSound.summaryInformation.getJSON());
                PluginId = FormatJson(n.kontaktSound.pluginId.getJSON());
                ControllerAssignments = FormatJson(n.kontaktSound.controllerAssignments.getJSON());

                var ms = new MemoryStream();

                ms.Write(n.kontaktSound.pluginChunk.Chunk, 0, n.kontaktSound.pluginChunk.Chunk.Length);

                PluginChunk = ms;
            }

            Log.Debug("Parse Complete");
        }

        #endregion Commands

        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
}