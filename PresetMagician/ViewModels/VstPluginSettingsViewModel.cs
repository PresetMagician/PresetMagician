using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Anotar.Catel;
using Catel;
using Catel.Collections;
using Catel.Fody;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Drachenkatze.PresetMagician.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Models.NativeInstrumentsResources;
using PresetMagician.Core.Services;
using PresetMagician.Models.ControllerAssignments;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.ViewModels
{
    public class VstPluginSettingsViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly IVstService _vstService;
        private readonly IOpenFileService _openFileService;
        private readonly ISelectDirectoryService _selectDirectoryService;
        private readonly ILicenseService _licenseService;
        private readonly IAdvancedMessageService _messageService;
        private readonly ICommandManager _commandManager;
        private readonly RemoteVstService _remoteVstService;
        private readonly INativeInstrumentsResourceGeneratorService _resourceGeneratorService;


        public VstPluginSettingsViewModel(Plugin plugin, IVstService vstService, IOpenFileService openFileService,
            ISelectDirectoryService selectDirectoryService, ILicenseService licenseService,
            IAdvancedMessageService messageService,
            ICommandManager commandManager,
            RemoteVstService remoteVstService,
            INativeInstrumentsResourceGeneratorService
                resourceGeneratorService)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => openFileService);
            Argument.IsNotNull(() => selectDirectoryService);
            Argument.IsNotNull(() => licenseService);
            Argument.IsNotNull(() => resourceGeneratorService);
            Argument.IsNotNull(() => commandManager);

            Plugin = plugin;

            _openFileService = openFileService;
            _commandManager = commandManager;
            _selectDirectoryService = selectDirectoryService;
            _licenseService = licenseService;
            _vstService = vstService;
            _resourceGeneratorService = resourceGeneratorService;
            _messageService = messageService;
            _remoteVstService = remoteVstService;

            OpenNKSFile = new TaskCommand(OnOpenNKSFileExecute);
            ClearMappings = new TaskCommand(OnClearMappingsExecute);
            AddAdditionalPresetFiles = new TaskCommand(OnAddAdditionalPresetFilesExecute);
            AddAdditionalPresetFolder = new TaskCommand(OnAddAdditionalPresetFolderExecute);
            RemoveAdditionalBankFiles = new Command<object>(OnRemoveAdditionalBankFilesExecute);
            RemoveCategory = new Command<object>(OnRemoveCategoryExecute);
            AddCategory = new Command(OnAddCategoryExecute);

            ReplaceVBLogo = new TaskCommand(OnReplaceVBLogoExecute);
            ReplaceVBArtwork = new TaskCommand(OnReplaceVBArtworkExecute);

            ReplaceMSTLogo = new TaskCommand(OnReplaceMSTLogoExecute);
            ReplaceMSTArtwork = new TaskCommand(OnReplaceMSTArtworkExecute);
            ReplaceMSTPlugin = new TaskCommand(OnReplaceMSTPluginExecute);

            ReplaceOSOLogo = new TaskCommand(OnReplaceOSOLogoExecute);

            SubmitResource = new TaskCommand(OnSubmitResourceExecute);
            QueryOnlineResources = new TaskCommand(OnQueryOnlineResourcesExecute);
            LoadSelectedOnlineResource = new TaskCommand(OnLoadSelectedOnlineResourceExecute);

            GenerateResources = new TaskCommand(OnGenerateResourcesExecute);


            Title = "Settings for " + Plugin.PluginName;


            GenerateControllerMappingModels();
            PluginLocations = (from pluginLocation in Plugin.PluginLocations
                where pluginLocation.IsPresent
                select pluginLocation).ToList();
        }


        #region Properties

        public ObservableCollection<OnlineResource> OnlineResources { get; set; } =
            new ObservableCollection<OnlineResource>();

        public OnlineResource SelectedOnlineResource { get; set; }

        public List<PluginLocation> PluginLocations { get; }

        public ObservableCollection<ControllerAssignmentPage> ControllerAssignmentPages { get; set; } =
            new ObservableCollection<ControllerAssignmentPage>();

        public int CurrentControllerAssignmentPage { get; set; }

        public bool ReanalyzePluginOnClose { get; set; }

        public bool IsPluginSet => Plugin != null;

        [Model]
        [Expose("AudioPreviewPreDelay")]
        public Plugin Plugin { get; protected set; }

        [ViewModelToModel("Plugin")] public ControllerAssignments DefaultControllerAssignments { get; set; }

        [ViewModelToModel("Plugin")] public NativeInstrumentsResource NativeInstrumentsResource { get; set; }

        [ViewModelToModel("Plugin")] public FastObservableCollection<BankFile> AdditionalBankFiles { get; set; }

        #endregion

        #region Commands

        #region LoadSelectedOnlineResource

        public TaskCommand LoadSelectedOnlineResource { get; set; }

        private async Task OnLoadSelectedOnlineResourceExecute()
        {
            var client = new HttpClient();

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.

            try
            {
                var response = await client.GetAsync(Settings.Links.GetOnlineResource + SelectedOnlineResource.id);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var part = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(part);

                    NativeInstrumentsResource.LoadFromJObject(data);
                }
            }
            catch (HttpRequestException e)
            {
                Log.Error($"Unable to load online resources, error {e.ToString()}");
                Log.Debug(e);
            }
        }

        #endregion

        #region OpenNKSFile

        public TaskCommand OpenNKSFile { get; set; }

        private async Task OnOpenNKSFileExecute()
        {
            try
            {
                _openFileService.Filter = "NKS Files (*.nksf,*.nksfx)|*.nksf;*.nksfx";
                _openFileService.IsMultiSelect = false;

                if (await _openFileService.DetermineFileAsync())
                {
                    ApplyControllerMapping(_openFileService.FileName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }

        #endregion

        #region AddAdditionalPresetFiles

        public TaskCommand AddAdditionalPresetFiles { get; set; }

        private async Task OnAddAdditionalPresetFilesExecute()
        {
            List<(string fileName, string reason)> FailedFiles = new List<(string, string)>();
            try
            {
                _openFileService.Filter = "Bank/Preset Files (*.fxb;*.fxp)|*.fxb;*.fxp";
                _openFileService.FilterIndex = 0;
                _openFileService.IsMultiSelect = true;

                if (await _openFileService.DetermineFileAsync())
                {
                    await AddFxbFxpFiles(_openFileService.FileNames.ToList());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }

        #endregion

        #endregion

        protected override async Task<bool> SaveAsync()
        {
            try
            {
                _vstService.Save();
            }
            catch (Exception e)
            {
                LogTo.Error($"Error occured while saving plugin settings: {e.Message}");
                LogTo.Debug(e.StackTrace);
            }

            var result = await base.SaveAsync();

            if (NativeInstrumentsResource.ColorState.State == NativeInstrumentsResource.ResourceStates.Empty &&
                !NativeInstrumentsResource.Color.BackgroundColor.Equals(Colors.White))
            {
                NativeInstrumentsResource.ColorState.State = NativeInstrumentsResource.ResourceStates.UserModified;
            }

            if (NativeInstrumentsResource.Categories.CategoryNames.Count != 0)
            {
                NativeInstrumentsResource.CategoriesState.State =
                    NativeInstrumentsResource.ResourceStates.UserModified;
            }

            if (NativeInstrumentsResource.ShortNamesState.State == NativeInstrumentsResource.ResourceStates.Empty &&
                (!string.IsNullOrEmpty(NativeInstrumentsResource.ShortNames.VB_shortname) ||
                 !string.IsNullOrEmpty(NativeInstrumentsResource.ShortNames.MST_shortname) ||
            !string.IsNullOrEmpty(NativeInstrumentsResource.ShortNames.MKII_shortname) ||
           !string.IsNullOrEmpty(NativeInstrumentsResource.ShortNames.MIKRO_shortname)
                 )) {
                NativeInstrumentsResource.ShortNamesState.State =
                    NativeInstrumentsResource.ResourceStates.UserModified;
            }
            NativeInstrumentsResource.Save(Plugin);


            if (ReanalyzePluginOnClose)
            {
                _commandManager.ExecuteCommand(Commands.Plugin.ScanSelectedPlugin);
            }

            return result;
        }


        private List<string> GetFxbFxpFiles(string path, List<string> patterns)
        {
            List<string> files = new List<string>();
            var directory = new DirectoryInfo(path);

            foreach (var pattern in patterns)
            {
                var results = directory.EnumerateFiles(pattern, SearchOption.AllDirectories);

                foreach (var result in results)
                {
                    if (!files.Contains(result.FullName))
                    {
                        files.Add(result.FullName);
                    }
                }
            }

            return files;
        }


        public TaskCommand AddAdditionalPresetFolder { get; set; }

        private async Task OnAddAdditionalPresetFolderExecute()
        {
            try
            {
                if (await _selectDirectoryService.DetermineDirectoryAsync())
                {
                    var files = GetFxbFxpFiles(_selectDirectoryService.DirectoryName,
                        new List<string> {"*.fxp", "*.fxb"});

                    await AddFxbFxpFiles(files);

                    ReanalyzePluginOnClose = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }

        public async Task AddFxbFxpFiles(List<string> files)
        {
            List<(string fileName, string reason)> failedFiles = new List<(string, string)>();

            foreach (var filename in files)
            {
                var result = VstUtils.LoadFxp(filename, Plugin.PluginInfo.ToNonSurrogate());

                if (result.result == VstUtils.LoadFxpResult.Error)
                {
                    failedFiles.Add((filename, result.message));
                }
                else
                {
                    AddBankFile(filename);
                }
            }

            if (failedFiles.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("The following FXB/FXP files could not be added:");
                sb.AppendLine();

                foreach (var fail in failedFiles)
                {
                    sb.AppendLine($"{fail.fileName}: {fail.reason}");
                    sb.AppendLine();
                }

                await _messageService.ShowErrorAsync(sb.ToString(), "Unable to add some FXB/FXP files",
                    HelpLinks.SETTINGS_PLUGIN_FXBFXPNOTES);
            }
        }

        public TaskCommand ReplaceVBLogo { get; set; }

        private async Task OnReplaceVBLogoExecute()
        {
            try
            {
                _openFileService.Filter = "PNG File (*.png)|*.png";
                _openFileService.IsMultiSelect = false;

                if (await _openFileService.DetermineFileAsync())
                {
                    NativeInstrumentsResource.VB_logo.ReplaceFromFile(_openFileService.FileName,
                        NativeInstrumentsResource.ResourceStates.UserModified);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }

        public TaskCommand ReplaceVBArtwork { get; set; }

        private async Task OnReplaceVBArtworkExecute()
        {
            try
            {
                _openFileService.Filter = "PNG File (*.png)|*.png";
                _openFileService.IsMultiSelect = false;

                if (await _openFileService.DetermineFileAsync())
                {
                    NativeInstrumentsResource.VB_artwork.ReplaceFromFile(_openFileService.FileName,
                        NativeInstrumentsResource.ResourceStates.UserModified);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }

        public TaskCommand ReplaceMSTArtwork { get; set; }

        private async Task OnReplaceMSTArtworkExecute()
        {
            try
            {
                _openFileService.Filter = "PNG File (*.png)|*.png";
                _openFileService.IsMultiSelect = false;

                if (await _openFileService.DetermineFileAsync())
                {
                    NativeInstrumentsResource.MST_artwork.ReplaceFromFile(_openFileService.FileName,
                        NativeInstrumentsResource.ResourceStates.UserModified);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }

        public TaskCommand ReplaceMSTLogo { get; set; }

        private async Task OnReplaceMSTLogoExecute()
        {
            try
            {
                _openFileService.Filter = "PNG File (*.png)|*.png";
                _openFileService.IsMultiSelect = false;

                if (await _openFileService.DetermineFileAsync())
                {
                    NativeInstrumentsResource.MST_logo.ReplaceFromFile(_openFileService.FileName,
                        NativeInstrumentsResource.ResourceStates.UserModified);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }

        public TaskCommand ReplaceMSTPlugin { get; set; }

        private async Task OnReplaceMSTPluginExecute()
        {
            try
            {
                _openFileService.Filter = "PNG File (*.png)|*.png";
                _openFileService.IsMultiSelect = false;

                if (await _openFileService.DetermineFileAsync())
                {
                    NativeInstrumentsResource.MST_plugin.ReplaceFromFile(_openFileService.FileName,
                        NativeInstrumentsResource.ResourceStates.UserModified);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }

        public TaskCommand SubmitResource { get; set; }

        private async Task OnSubmitResourceExecute()
        {
            var niResource = NativeInstrumentsResource;

            List<string> categoryNames = new List<string>();

            foreach (var category in niResource.Categories.CategoryNames)
            {
                categoryNames.Add(category.Name);
            }


            var resourceSubmission = JObject.FromObject(new
            {
                email = _licenseService.GetCurrentLicense().Customer.Email,
                pluginId = Plugin.VstPluginId.ToString(),
                bgColor = niResource.Color.VB_bgcolor,
                shortName_VB = niResource.ShortNames.VB_shortname,
                shortName_MST = niResource.ShortNames.MST_shortname,
                shortName_MKII = niResource.ShortNames.MKII_shortname,
                shortName_MIKRO = niResource.ShortNames.MIKRO_shortname,
                categories = string.Join(",", categoryNames),
                image_VB_logo = niResource.VB_logo.ToBase64(),
                image_VB_artwork = niResource.VB_artwork.ToBase64(),
                image_MST_logo = niResource.MST_logo.ToBase64(),
                image_MST_artwork = niResource.MST_artwork.ToBase64(),
                image_MST_plugin = niResource.MST_plugin.ToBase64(),
                image_OSO_logo = niResource.OSO_logo.ToBase64()
            });

            HttpContent content = new StringContent(resourceSubmission.ToString());

            var client = new HttpClient();

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.

            try
            {
                var response = await client.PostAsync(Settings.Links.SubmitResource, content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                }
                else
                {
                    LogTo.Error("Error submitting resource");
                    LogTo.Debug(response.Content.ToString());
                }
            }
            catch (HttpRequestException e)
            {
                LogTo.Error($"Error submitting resource: {e.Message}");
                LogTo.Debug(e.StackTrace);
            }
            catch (SocketException e)
            {
                LogTo.Error($"Error submitting resource: {e.Message}");
                LogTo.Debug(e.StackTrace);
            }
            catch (Exception e)
            {
                LogTo.Error($"Error submitting resource: {e.Message}");
                LogTo.Debug(e.StackTrace);
            }
        }

        public TaskCommand QueryOnlineResources { get; set; }

        private async Task OnQueryOnlineResourcesExecute()
        {
            var client = new HttpClient();

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.

            try
            {
                var response = await client.GetAsync(Settings.Links.GetOnlineResources + Plugin.VstPluginId);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var part = await response.Content.ReadAsStringAsync();

                    OnlineResources = JsonConvert.DeserializeObject<ObservableCollection<OnlineResource>>(part);
                }
            }
            catch (HttpRequestException e)
            {
            }
        }

        public TaskCommand ReplaceOSOLogo { get; set; }

        private async Task OnReplaceOSOLogoExecute()
        {
            try
            {
                _openFileService.Filter = "PNG File (*.png)|*.png";
                _openFileService.IsMultiSelect = false;

                if (await _openFileService.DetermineFileAsync())
                {
                    NativeInstrumentsResource.OSO_logo.ReplaceFromFile(_openFileService.FileName,
                        NativeInstrumentsResource.ResourceStates.UserModified);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }

        public TaskCommand GenerateResources { get; set; }

        private async Task OnGenerateResourcesExecute()
        {
            using (var remotePluginInstance = _remoteVstService.GetRemotePluginInstance(Plugin))
            {
                await remotePluginInstance.LoadPlugin();
                remotePluginInstance.OpenEditorHidden();
                await Task.Delay(1000);
                _resourceGeneratorService.GenerateResources(remotePluginInstance, true);
                remotePluginInstance.UnloadPlugin();
            }
        }

        private void AddBankFile(string path)
        {
            if ((from bankFile in AdditionalBankFiles where bankFile.Path == path select bankFile)
                .Any())
            {
                return;
            }

            var bankName = Path.GetExtension(path) == ".fxp" ? "User Presets" : Path.GetFileNameWithoutExtension(path);

            AdditionalBankFiles.Add(new BankFile {Path = path, BankName = bankName});
            ReanalyzePluginOnClose = true;
        }

        public Command<object> RemoveAdditionalBankFiles { get; set; }

        private void OnRemoveAdditionalBankFilesExecute(object parameter)
        {
            var folders = (parameter as IList).Cast<BankFile>();

            foreach (var folder in folders.ToList())
            {
                AdditionalBankFiles.Remove(folder);
            }
        }

        public Command<object> RemoveCategory { get; set; }

        private void OnRemoveCategoryExecute(object parameter)
        {
            var categories = (parameter as IList).Cast<Category>();

            foreach (var category in categories.ToList())
            {
                NativeInstrumentsResource.Categories.CategoryNames.Remove(category);
            }
        }

        public Command AddCategory { get; set; }

        private void OnAddCategoryExecute()
        {
            NativeInstrumentsResource.Categories.CategoryNames.Add(new Category {Name = "New Category"});
        }

        public TaskCommand ClearMappings { get; set; }

        private async Task OnClearMappingsExecute()
        {
            DefaultControllerAssignments = null;
            GenerateControllerMappingModels();
        }

        private void ApplyControllerMapping(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                NKSFRiff n = new NKSFRiff();
                n.Read(fileStream);

                DefaultControllerAssignments =
                    n.kontaktSound.controllerAssignments.controllerAssignments;
            }

            GenerateControllerMappingModels();
        }

        private void GenerateControllerMappingModels()
        {
            if (!IsPluginSet)
            {
                return;
            }

            var controllerAssignmentPages = new ObservableCollection<ControllerAssignmentPage>();

            if (DefaultControllerAssignments != null)
            {
                foreach (var page in DefaultControllerAssignments.controllerAssignments)
                {
                    var controllerAssignmentPage = new ControllerAssignmentPage();
                    var titles = new List<string>();
                    foreach (var control in page)
                    {
                        var newControl = new ControllerAssignmentControl(control);

                        if (!string.IsNullOrEmpty(control.section))
                        {
                            titles.Add(control.section);
                        }

                        if (controllerAssignmentPage.Controls.Count > 0 &&
                            controllerAssignmentPage.Controls.Last().section == null && control.section != null)
                        {
                            controllerAssignmentPage.Controls.Last().LastSectionItem = true;
                        }

                        controllerAssignmentPage.Controls.Add(newControl);
                    }

                    if (titles.Count == 0)
                    {
                        controllerAssignmentPage.Title =
                            $"Page {DefaultControllerAssignments.controllerAssignments.IndexOf(page)}";
                    }
                    else
                    {
                        controllerAssignmentPage.Title = string.Join(" / ", titles);
                    }


                    controllerAssignmentPages.Add(controllerAssignmentPage);
                }
            }

            ControllerAssignmentPages = controllerAssignmentPages;
            RaisePropertyChanged(nameof(ControllerAssignmentPages));
            CurrentControllerAssignmentPage = 0;
        }
    }
}