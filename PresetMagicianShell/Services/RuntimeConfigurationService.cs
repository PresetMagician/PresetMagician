using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Catel.IoC;
using Catel.Logging;
using Catel.Runtime.Serialization.Json;
using Newtonsoft.Json;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;
using Path = Catel.IO.Path;

namespace PresetMagicianShell.Services
{
    public class RuntimeConfigurationService : IRuntimeConfigurationService
    {
        private static readonly string DefaultLocalConfigFilePath = Path.Combine(Path.GetApplicationDataDirectory(Catel.IO.ApplicationDataTarget.UserLocal), "configuration.json");
        
        private readonly JsonSerializationConfiguration _jsonSerializerConfiguration;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();

        public RuntimeConfiguration RuntimeConfiguration { get; }
        
        public RuntimeConfigurationService(IJsonSerializer jsonSerializer)
        {
            RuntimeConfiguration = new RuntimeConfiguration();
            _jsonSerializer = jsonSerializer;
            
            _jsonSerializerConfiguration= new JsonSerializationConfiguration
            {
                Formatting = Formatting.Indented,
                UseBson = false
            };
        }
        
        public void LoadConfiguration()
        {
            if (!File.Exists(DefaultLocalConfigFilePath))
            {
                _logger.Info("No configuration found.");
                return;
            }
            
            var configurationFile = new FileStream(DefaultLocalConfigFilePath, FileMode.Open);
            
            _jsonSerializer.WriteTypeInfo = false;
            _jsonSerializer.PreserveReferences = false;

            _jsonSerializer.Deserialize(RuntimeConfiguration, configurationFile, _jsonSerializerConfiguration);
            
            configurationFile.Close();
        }

        public void SaveConfiguration()
        {
            var configurationFile = new FileStream(DefaultLocalConfigFilePath, FileMode.Create);
            
            _jsonSerializer.WriteTypeInfo = false;
            _jsonSerializer.PreserveReferences = false;

            _jsonSerializer.Serialize(RuntimeConfiguration, configurationFile, _jsonSerializerConfiguration);
            
            configurationFile.Close();
        }
    }
}