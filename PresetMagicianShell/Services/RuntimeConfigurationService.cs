using System;
using System.IO;
using Catel.IoC;
using Catel.IO;
using Catel.Logging;
using Catel.Runtime.Serialization.Json;
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

        private readonly JsonSerializationConfiguration _jsonSerializerConfiguration;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IServiceLocator _serviceLocator;
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();

        private LayoutRoot originalLayout;
        
        public RuntimeConfiguration RuntimeConfiguration { get; }

        public RuntimeConfigurationService(IJsonSerializer jsonSerializer, IServiceLocator serviceLocator)
        {
            RuntimeConfiguration = new RuntimeConfiguration();
            _serviceLocator = serviceLocator;
            _jsonSerializer = jsonSerializer;
            _jsonSerializer.WriteTypeInfo = false;
            _jsonSerializer.PreserveReferences = false;

            _jsonSerializerConfiguration = new JsonSerializationConfiguration
            {
                Formatting = Formatting.Indented,
                UseBson = false
            };
        }

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

            var configurationFile = new FileStream(DefaultLocalConfigFilePath, FileMode.Open);

            _jsonSerializer.Deserialize(RuntimeConfiguration, configurationFile, _jsonSerializerConfiguration);

            configurationFile.Close();
        }

        public void LoadLayout()
        {
            originalLayout = getDockingManager().Layout;
            
            if (File.Exists(DefaultLocalLayoutFilePath)) {
                try
                {
                    getLayoutSerializer().Deserialize(DefaultLocalLayoutFilePath);
                }
                catch (Exception)
                {
                    // Probably something wrong with the file, ignore
                }
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
            var configurationFile = new FileStream(DefaultLocalConfigFilePath, FileMode.Create);

            _jsonSerializer.Serialize(RuntimeConfiguration, configurationFile, _jsonSerializerConfiguration);

            configurationFile.Close();
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