using System;
using System.Diagnostics;
using Catel.MVVM;
using System.IO;
using Catel;
using Catel.Logging;
using Catel.Services;
using Orc.FileSystem;
using System.Threading.Tasks;
using Catel.IoC;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Newtonsoft.Json;
using Orchestra.Services;

namespace PresetMagician.ViewModels
{
    public class NKSFViewModel : ViewModelBase
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IOpenFileService _openFileService;

        #endregion Fields

        public string SummaryInformation { get; private set; }
        public string ControllerAssignments { get; private set; }
        public string PluginId { get; private set; }
        public MemoryStream PluginChunk { get; set; }
        private string filePath { get; set; }

        public NKSFViewModel(IOpenFileService openFileService)
        {
            Argument.IsNotNull(() => openFileService);
            

            _openFileService = openFileService;
           

            Title = "NKSF Viewer";

            OpenNKSFFile = new TaskCommand(OnOpenNKSFFileExecute);
            OpenWithHxD = new TaskCommand(OnOpenWithHxDExecute);
            
        }

        #region Commands

        public TaskCommand OpenNKSFFile { get; set; }

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
        
        public TaskCommand OpenWithHxD { get; set; }

        private async Task OnOpenWithHxDExecute ()
        {
            
            var tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, PluginChunk.ToArray());
          
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

        #endregion Commands

        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
}