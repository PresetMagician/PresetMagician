using System;
using System.IO;
using Catel.IO;
using Catel.Logging;
using Newtonsoft.Json;
using PresetMagician.Models;
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

        private static readonly string _defaultLocalLayoutFilePath =
            Path.Combine(Path.GetApplicationDataDirectory(ApplicationDataTarget.UserRoaming), "layout.xml");

        private readonly JsonSerializer _jsonSerializer;
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();

        public RuntimeConfigurationService()
        {
            RuntimeConfiguration = new RuntimeConfiguration();
            ApplicationState = new ApplicationState();

            _jsonSerializer = new JsonSerializer {Formatting = Formatting.Indented};
        }

        public RuntimeConfiguration RuntimeConfiguration { get; private set; }
        public RuntimeConfiguration EditableConfiguration { get; private set; }
        public ApplicationState ApplicationState { get; private set; }

        public void Load()
        {
            LoadConfiguration();
        }

        public void CreateEditableConfiguration()
        {
            string output = JsonConvert.SerializeObject(RuntimeConfiguration);

            EditableConfiguration = JsonConvert.DeserializeObject<RuntimeConfiguration>(output);
        }

        public void ApplyEditableConfiguration()
        {
            string output = JsonConvert.SerializeObject(EditableConfiguration);

            using (RuntimeConfiguration.SuspendValidations())
            {
                RuntimeConfiguration.CachedPlugins.Clear();
                RuntimeConfiguration.VstDirectories.Clear();
                JsonConvert.PopulateObject(output, RuntimeConfiguration);
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
                    using (RuntimeConfiguration.SuspendValidations())
                    {
                        RuntimeConfiguration.CachedPlugins.Clear();
                        RuntimeConfiguration.VstDirectories.Clear();
                        _jsonSerializer.Populate(jsonReader, RuntimeConfiguration);
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
                _jsonSerializer.Serialize(jsonWriter, RuntimeConfiguration);
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