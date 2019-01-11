using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading.Tasks;
using Anotar.Catel;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Drachenkatze.PresetMagician.NKSF.NKSF;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PresetMagician.Models;
using PresetMagician.Models.ControllerAssignments;
using PresetMagician.Models.NativeInstrumentsResources;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.ViewModels
{
    public class VstPluginViewModel : ViewModelBase
    {
        protected static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly IVstService _vstService;
        protected readonly IOpenFileService _openFileService;
        private readonly ISelectDirectoryService _selectDirectoryService;
        private readonly ILicenseService _licenseService;

        public ObservableCollection<OnlineResource> OnlineResources { get; set; } =
            new ObservableCollection<OnlineResource>();

        public OnlineResource SelectedOnlineResource { get; set; }

        public VstPluginViewModel(Plugin plugin, IVstService vstService, IOpenFileService openFileService,
            ISelectDirectoryService selectDirectoryService, ILicenseService licenseService)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => openFileService);
            Argument.IsNotNull(() => selectDirectoryService);
            Argument.IsNotNull(() => licenseService);

            Plugin = plugin;

            _openFileService = openFileService;
            _selectDirectoryService = selectDirectoryService;
            _licenseService = licenseService;

            OpenNKSFFile = new TaskCommand(OnOpenNKSFFileExecute);
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

            Title = "Settings for " + Plugin.PluginName;

            LoadNativeInstrumentsResources();
        }

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
            }
        }

        protected override async Task<bool> SaveAsync()
        {
            NativeInstrumentsResource.Save(Plugin);

            return await base.SaveAsync();
        }

        public ObservableCollection<ControllerAssignmentPage> ControllerAssignmentPages { get; set; } =
            new ObservableCollection<ControllerAssignmentPage>();

        public int CurrentControllerAssignmentPage { get; set; }


        public TaskCommand OpenNKSFFile { get; set; }

        private async Task OnOpenNKSFFileExecute()
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


        private List<string> GetFiles(string path, List<string> patterns)
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

        public TaskCommand AddAdditionalPresetFiles { get; set; }

        private async Task OnAddAdditionalPresetFilesExecute()
        {
            try
            {
                _openFileService.Filter = "Bank/Preset Files (*.fxb;*.fxp)|*.fxb;*.fxp";
                _openFileService.FilterIndex = 0;
                _openFileService.IsMultiSelect = true;

                if (await _openFileService.DetermineFileAsync())
                {
                    foreach (var filename in _openFileService.FileNames)
                    {
                        AddBankFile(filename);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }

        public TaskCommand AddAdditionalPresetFolder { get; set; }

        private async Task OnAddAdditionalPresetFolderExecute()
        {
            try
            {
                if (await _selectDirectoryService.DetermineDirectoryAsync())
                {
                    var files = GetFiles(_selectDirectoryService.DirectoryName, new List<string> {"*.fxp", "*.fxb"});

                    foreach (var filename in files)
                    {
                        AddBankFile(filename);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
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
                    NativeInstrumentsResource.VB_logo =
                        NativeInstrumentsResource.ReplaceImage(_openFileService.FileName,
                            NativeInstrumentsResource.VB_logoStream);
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
                    NativeInstrumentsResource.VB_artwork =
                        NativeInstrumentsResource.ReplaceImage(_openFileService.FileName,
                            NativeInstrumentsResource.VB_artworkStream);
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
                    NativeInstrumentsResource.MST_artwork =
                        NativeInstrumentsResource.ReplaceImage(_openFileService.FileName,
                            NativeInstrumentsResource.MST_artworkStream);
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
                    NativeInstrumentsResource.MST_logo =
                        NativeInstrumentsResource.ReplaceImage(_openFileService.FileName,
                            NativeInstrumentsResource.MST_logoStream);
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
                    NativeInstrumentsResource.MST_plugin =
                        NativeInstrumentsResource.ReplaceImage(_openFileService.FileName,
                            NativeInstrumentsResource.MST_pluginStream);
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
            List<string> categoryNames = new List<string>();
            
            foreach (var category in NativeInstrumentsResource.Categories.CategoryNames)
            {
                categoryNames.Add(category.Name);
            }
                        
            var resourceSubmission = JObject.FromObject(new
            {
                email = _licenseService.GetCurrentLicense().Customer.Email,
                pluginId = Plugin.PluginId.ToString(),
                bgColor = NativeInstrumentsResource.Color.VB_bgcolor,
                shortName_VB = NativeInstrumentsResource.ShortNames.VB_shortname,
                shortName_MST = NativeInstrumentsResource.ShortNames.MST_shortname,
                shortName_MKII = NativeInstrumentsResource.ShortNames.MKII_shortname,
                shortName_MIKRO = NativeInstrumentsResource.ShortNames.MIKRO_shortname,
                categories = string.Join(",", categoryNames),
                image_VB_logo = Convert.ToBase64String(NativeInstrumentsResource.VB_logoStream.ToArray()),
                image_VB_artwork = Convert.ToBase64String(NativeInstrumentsResource.VB_artworkStream.ToArray()),
                image_MST_logo = Convert.ToBase64String(NativeInstrumentsResource.MST_logoStream.ToArray()),
                image_MST_artwork = Convert.ToBase64String(NativeInstrumentsResource.MST_artworkStream.ToArray()),
                image_MST_plugin = Convert.ToBase64String(NativeInstrumentsResource.MST_pluginStream.ToArray()),
                image_OSO_logo = Convert.ToBase64String(NativeInstrumentsResource.OSO_logoStream.ToArray())
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
            }catch (Exception e)
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
                var response = await client.GetAsync(Settings.Links.GetOnlineResources + Plugin.PluginId);

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
                    NativeInstrumentsResource.OSO_logo =
                        NativeInstrumentsResource.ReplaceImage(_openFileService.FileName,
                            NativeInstrumentsResource.OSO_logoStream);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open file");
            }
        }

        private void AddBankFile(string path)
        {
            if (!(from bankFile in Plugin.Configuration.AdditionalBankFiles where bankFile.Path == path select bankFile)
                .Any())
            {
                string bankName;

                if (Path.GetExtension(path) == ".fxp")
                {
                    bankName = "User Presets";
                }
                else
                {
                    bankName = Path.GetFileNameWithoutExtension(path);
                }

                Plugin.Configuration.AdditionalBankFiles.Add(new BankFile {Path = path, BankName = bankName});
            }
        }

        public Command<object> RemoveAdditionalBankFiles { get; set; }

        private void OnRemoveAdditionalBankFilesExecute(object parameter)
        {
            var folders = (parameter as IList).Cast<BankFile>();

            foreach (var folder in folders.ToList())
            {
                Plugin.Configuration.AdditionalBankFiles.Remove(folder);
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
            Plugin.Configuration.DefaultControllerAssignments = null;
            GenerateControllerMappingModels();
        }

        private void ApplyControllerMapping(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                NKSFRiff n = new NKSFRiff();
                n.Read(fileStream);

                Plugin.Configuration.DefaultControllerAssignments =
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

            if (Plugin.Configuration.DefaultControllerAssignments != null)
            {
                foreach (var page in Plugin.Configuration.DefaultControllerAssignments.controllerAssignments)
                {
                    var controllerAssignmentPage = new ControllerAssignmentPage();
                    List<string> titles = new List<string>();
                    foreach (var control in page)
                    {
                        var newControl = new ControllerAssignmentControl(control);

                        if (control.section != null)
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
                            $"Page {Plugin.Configuration.DefaultControllerAssignments.controllerAssignments.IndexOf(page)}";
                    }
                    else
                    {
                        controllerAssignmentPage.Title = String.Join(" / ", titles);
                    }


                    controllerAssignmentPages.Add(controllerAssignmentPage);
                }
            }

            ControllerAssignmentPages = controllerAssignmentPages;
            RaisePropertyChanged(nameof(ControllerAssignmentPages));
            CurrentControllerAssignmentPage = 0;
        }

        public bool IsPluginSet
        {
            get { return Plugin != null; }
        }

        public Plugin Plugin { get; protected set; }
        public NativeInstrumentsResource NativeInstrumentsResource { get; protected set; }

        public void LoadNativeInstrumentsResources()
        {
            NativeInstrumentsResource = new NativeInstrumentsResource();
            NativeInstrumentsResource.Load(Plugin);
        }
    }
}