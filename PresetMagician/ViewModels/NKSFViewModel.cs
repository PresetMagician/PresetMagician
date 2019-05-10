using System;
using System.IO;
using System.Threading.Tasks;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Newtonsoft.Json;
using PresetMagician.Core.Services;
using PresetMagician.NKS;

namespace PresetMagician.ViewModels
{
    public class NKSFViewModel : ViewModelBase
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IOpenFileService _openFileService;
        private readonly DeveloperService _developerService;

        #endregion Fields

        public string SummaryInformation { get; private set; }
        public string ControllerAssignments { get; private set; }
        public string PluginId { get; private set; }
        public MemoryStream PluginChunk { get; set; }
        private string filePath { get; set; }

        public NKSFViewModel(IOpenFileService openFileService, DeveloperService developerService)
        {
            Argument.IsNotNull(() => openFileService);


            _openFileService = openFileService;
            _developerService = developerService;


            Title = "NKS Viewer";

            OpenNKSFile = new TaskCommand(OnOpenNKSFFileExecute);
            OpenWithHexEditor = new TaskCommand(OnOpenWithHexEditorExecute);
        }

        #region Commands

        public TaskCommand OpenNKSFile { get; set; }

        private async Task OnOpenNKSFFileExecute()
        {
            try
            {
                _openFileService.Filter = "NKS Files (*.nksf,*.nksfx)|*.nksf;*.nksfx";
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
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
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

        public TaskCommand OpenWithHexEditor { get; set; }

        private async Task OnOpenWithHexEditorExecute()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, PluginChunk.ToArray());
            _developerService.StartHexEditor(tempFile);
        }

        #endregion Commands

        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
}