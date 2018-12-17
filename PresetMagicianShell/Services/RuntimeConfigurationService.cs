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
        private static readonly string DefaultLocalConfigFilePath =
            Path.Combine(Path.GetApplicationDataDirectory(ApplicationDataTarget.UserLocal),
                "configuration.json");

        private static readonly string DefaultLocalLayoutFilePath =
            Path.Combine(Path.GetApplicationDataDirectory(ApplicationDataTarget.UserLocal), "layout.xml");

        private readonly JsonSerializer _jsonSerializer;
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();
        private readonly IServiceLocator _serviceLocator;

        private LayoutRoot originalLayout;

        public RuntimeConfigurationService(IServiceLocator serviceLocator)
        {
            RuntimeConfiguration = new RuntimeConfiguration();
            _serviceLocator = serviceLocator;
            _jsonSerializer = new JsonSerializer();
            _jsonSerializer.Formatting = Formatting.Indented;
        }

        public RuntimeConfiguration RuntimeConfiguration { get; private set; }

        public void Load()
        {
            LoadConfiguration();
        }

        public void LoadConfiguration()
        {
            if (!File.Exists(DefaultLocalConfigFilePath))
            {
                _logger.Info("No configuration found.");
                return;
            }

            try
            {
                using (var rd = new StreamReader(DefaultLocalConfigFilePath))
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

            originalLayout = getDockingManager().Layout;

            if (File.Exists(DefaultLocalLayoutFilePath))
                try
                {
                    getLayoutSerializer().Deserialize(DefaultLocalLayoutFilePath);
                }
                catch (Exception)
                {
                    // Probably something wrong with the file, ignore
                }
        }

        public void ResetLayout()
        {
            var dockingManager = getDockingManager();

            dockingManager.Layout = originalLayout;
        }

        public void Save()
        {
            SaveConfiguration();
            SaveLayout();
        }

        public void SaveConfiguration()
        {
            using (var sw = new StreamWriter(DefaultLocalConfigFilePath))
            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                _jsonSerializer.Serialize(jsonWriter, RuntimeConfiguration);
            }
        }

        public void SaveLayout()
        {
            getLayoutSerializer().Serialize(DefaultLocalLayoutFilePath);
        }

        private XmlLayoutSerializer getLayoutSerializer()
        {
            return new XmlLayoutSerializer(getDockingManager());
        }

        private DockingManager getDockingManager()
        {
            return _serviceLocator.ResolveType<DockingManager>();
        }
    }
}