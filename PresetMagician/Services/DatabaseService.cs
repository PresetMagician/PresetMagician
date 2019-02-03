using System;
using System.IO;
using System.Threading.Tasks;
using Orc.Scheduling;
using PresetMagician.Services.Interfaces;
using SharedModels;

namespace PresetMagician.Services
{
    public interface IDatabaseService
    {
        ApplicationDatabaseContext Context { get; }
        IDataPersistence GetPresetDataStorer();
        void UpdateDatabaseSize();
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly ApplicationDatabaseContext _dbContext;
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;

        
        public DatabaseService(IRuntimeConfigurationService runtimeConfigurationService)
        {
            _dbContext = new ApplicationDatabaseContext();
            
            _runtimeConfigurationService = runtimeConfigurationService;
        }
        
        public void UpdateDatabaseSize()
        {
            var path = ApplicationDatabaseContext.GetDatabasePath();

            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                _runtimeConfigurationService.ApplicationState.DatabaseSize = fileInfo.Length;
            }
        }

        public ApplicationDatabaseContext Context
        {
            get { return _dbContext; }
        }

        public IDataPersistence GetPresetDataStorer()
        {
            return _dbContext;
        }
    }
}