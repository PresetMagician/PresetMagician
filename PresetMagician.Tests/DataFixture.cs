using System;
using System.IO;
using Catel.IoC;
using PresetMagician.Core.Services;
using PresetMagician.Legacy;
using Xunit;

namespace PresetMagician.Tests
{
    public class DataFixture : IDisposable
    {
        private string _className;

        public DataFixture()
        {
        }

        public void Setup(string className)
        {
            ApplicationDatabaseContext.DefaultDatabasePath = Path.Combine(Directory.GetCurrentDirectory(),
                $@"TestData\{className}\LegacyDatabases\LegacyDb.sqlite3");
            DataPersisterService.DefaultPluginStoragePath =
                Path.Combine(Directory.GetCurrentDirectory(), $@"TestData\{className}\Plugins");
            DataPersisterService.DefaultTypesCharacteristicsStoragePath =
                Path.Combine(Directory.GetCurrentDirectory(), $@"TestData\{className}");
            PresetDataPersisterService.DefaultDatabasePath = Path.Combine(Directory.GetCurrentDirectory(),
                $@"TestData\{className}\PresetData.sqlite3");

            Directory.CreateDirectory(DataPersisterService.DefaultPluginStoragePath);
            Directory.CreateDirectory(Path.GetDirectoryName(ApplicationDatabaseContext.DefaultDatabasePath));
            Directory.CreateDirectory(Path.GetDirectoryName(PresetDataPersisterService.DefaultDatabasePath));


            File.Delete(ApplicationDatabaseContext.DefaultDatabasePath);
            File.Copy(@"Resources\PresetMagician.test.sqlite3", ApplicationDatabaseContext.DefaultDatabasePath);
        }

        public void Dispose()
        {
            ServiceLocator.Default.ResolveType<PresetDataPersisterService>().CloseDatabase().Wait();
        }
    }
}