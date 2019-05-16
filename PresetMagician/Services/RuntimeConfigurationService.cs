using System;
using System.IO;
using Catel.IO;
using Catel.IoC;
using Catel.Logging;
using Newtonsoft.Json;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;
using Path = Catel.IO.Path;

namespace PresetMagician.Services
{
    public class RuntimeConfigurationService : IRuntimeConfigurationService
    {
        private static readonly string _defaultLocalConfigFilePath =
            Path.Combine(Path.GetApplicationDataDirectory(ApplicationDataTarget.UserRoaming),
                "configuration.json");

        private static readonly string _defaultLocalConfigBackupFilePath =
            Path.Combine(Path.GetApplicationDataDirectory(ApplicationDataTarget.UserRoaming),
                "configuration.backup.json");

        private readonly JsonSerializer _jsonSerializer;
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();
        private readonly GlobalService _globalService;
        private readonly GlobalFrontendService _globalFrontendService;
        private readonly AudioService _audioService;

        public RuntimeConfigurationService()
        {
            _globalFrontendService = ServiceLocator.Default.ResolveType<GlobalFrontendService>();
            _globalService = ServiceLocator.Default.ResolveType<GlobalService>();
            _audioService = ServiceLocator.Default.ResolveType<AudioService>();


            _jsonSerializer = new JsonSerializer {Formatting = Formatting.Indented};
        }

        public RuntimeConfiguration EditableConfiguration { get; private set; }

        public void Load()
        {
            LoadConfiguration();
        }

        public void CreateEditableConfiguration()
        {
            string output = JsonConvert.SerializeObject(_globalService.RuntimeConfiguration);

            EditableConfiguration = JsonConvert.DeserializeObject<RuntimeConfiguration>(output);
        }

        public void ApplyEditableConfiguration()
        {
            string output = JsonConvert.SerializeObject(EditableConfiguration);

            using (_globalService.RuntimeConfiguration.SuspendValidations())
            {
                _globalService.RuntimeConfiguration.VstDirectories.Clear();
                _globalService.RuntimeConfiguration.MidiInputDevices.Clear();
                JsonConvert.PopulateObject(output, _globalService.RuntimeConfiguration);
            }

            SaveConfiguration();
        }

        public void LoadConfiguration()
        {
            _logger.Debug("Loading configuration");
            if (!File.Exists(_defaultLocalConfigFilePath))
            {
                _logger.Info("No configuration found.");
                return;
            }

            try
            {
                using (var rd = new StreamReader(_defaultLocalConfigFilePath))
                using (JsonReader jsonReader = new JsonTextReader(rd))
                {
                    using (_globalService.RuntimeConfiguration.SuspendValidations())
                    {
                        _globalService.RuntimeConfiguration.VstDirectories.Clear();
                        _globalService.RuntimeConfiguration.MidiInputDevices.Clear();
                        _jsonSerializer.Populate(jsonReader, _globalService.RuntimeConfiguration);
                    }

                    if (_globalService.RuntimeConfiguration.AudioOutputDevice == null)
                    {
                        _globalService.RuntimeConfiguration.AudioOutputDevice = _audioService.GetDefaultAudioDevice();
                    }
                }

                _logger.Debug("Configuration loaded");
            }
            catch (Exception e)
            {
                _logger.Error("Unable to load configuration file, probably corrupt. Error:" + e.Message);
            }
        }


        public void Save()
        {
            SaveConfiguration();
        }

        public void SaveConfiguration()
        {
            File.Delete(_defaultLocalConfigBackupFilePath);

            if (File.Exists(_defaultLocalConfigFilePath))
            {
                File.Copy(_defaultLocalConfigFilePath, _defaultLocalConfigBackupFilePath);
            }

            using (var sw = new StreamWriter(_defaultLocalConfigFilePath))
            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                _jsonSerializer.Serialize(jsonWriter, _globalService.RuntimeConfiguration);
            }
        }

        public bool IsConfigurationValueEqual(object left, object right)
        {
            var leftJson = JsonConvert.SerializeObject(left);
            var rightJson = JsonConvert.SerializeObject(right);

            return leftJson.Equals(rightJson);
        }
    }
}