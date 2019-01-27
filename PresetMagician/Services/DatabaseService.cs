using SharedModels;

namespace PresetMagician.Services
{
    public interface IDatabaseService
    {
        ApplicationDatabaseContext Context { get; }
        IPresetDataStorer GetPresetDataStorer();
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly ApplicationDatabaseContext _dbContext;

        public DatabaseService()
        {
            _dbContext = new ApplicationDatabaseContext();
        }

        public ApplicationDatabaseContext Context
        {
            get { return _dbContext; }
        }

        public IPresetDataStorer GetPresetDataStorer()
        {
            return _dbContext;
        }
    }
}