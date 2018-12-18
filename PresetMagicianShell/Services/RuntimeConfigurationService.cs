using System;
using System.IO;
using Catel.IoC;
using Catel.IO;
using Catel.Logging;
using Newtonsoft.Json;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using Path = Catel.IO.Path;

namespace PresetMagicianShell.Services
{
    public class RuntimeConfigurationService : IRuntimeConfigurationService
    {
        private static readonly string _defaultLocalConfigFilePath =
            Path.Combine(Path.GetApplicationDataDirectory(ApplicationDataTarget.UserRoaming),
                "configuration.json");

        private static readonly string _defaultLocalLayoutFilePath =
            Path.Combine(Path.GetApplicationDataDirectory(ApplicationDataTarget.UserRoaming), "layout.xml");

        private readonly JsonSerializer _jsonSerializer;
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();
        private readonly IServiceLocator _serviceLocator;

        private LayoutRoot _originalLayout;

        public RuntimeConfigurationService(IServiceLocator serviceLocator)
        {
            RuntimeConfiguration = new RuntimeConfiguration();
            ApplicationState = new ApplicationState();
            _serviceLocator = serviceLocator;
            _jsonSerializer = new JsonSerializer {Formatting = Formatting.Indented};
        }

        public RuntimeConfiguration RuntimeConfiguration { get; private set; }
        public ApplicationState ApplicationState { get; private set; }

        public void Load()
        {
            LoadConfiguration();
        }

        public void LoadConfiguration()
        {
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
                    RuntimeConfiguration = _jsonSerializer.Deserialize<RuntimeConfiguration>(jsonReader);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Unable to load configuration file, probably corrupt. Error:" + e.Message);
            }
        }

        public void LoadLayout()
        {
            return;
            // Disabled because loading the layout causes no documents to be active

            _originalLayout = GetDockingManager().Layout;

            if (File.Exists(_defaultLocalLayoutFilePath))
                try
                {
                    GetLayoutSerializer().Deserialize(_defaultLocalLayoutFilePath);
                }
                catch (Exception)
                {
                    // Probably something wrong with the file, ignore
                }
        }

        public void ResetLayout()
        {
            var dockingManager = GetDockingManager();

            dockingManager.Layout = _originalLayout;
        }

        public void Save()
        {
            SaveConfiguration();
            SaveLayout();
        }

        public void SaveConfiguration()
        {
            using (var sw = new StreamWriter(_defaultLocalConfigFilePath))
            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                _jsonSerializer.Serialize(jsonWriter, RuntimeConfiguration);
            }
        }

        public void SaveLayout()
        {
            GetLayoutSerializer().Serialize(_defaultLocalLayoutFilePath);
        }

        private XmlLayoutSerializer GetLayoutSerializer()
        {
            return new XmlLayoutSerializer(GetDockingManager());
        }

        private DockingManager GetDockingManager()
        {
            return _serviceLocator.ResolveType<DockingManager>();
        }
    }
}