using System;
using System.Collections.Generic;
using Xunit;

namespace PresetMagician.Tests
{
    public class DatabaseFixture : IDisposable
    {
        private List<EmptyDbContextManager> dbManagers = new List<EmptyDbContextManager>();
        public DatabaseFixture()
        {
            
        }

        public EmptyDbContextManager GetEmptyManager()
        {
            var x = new EmptyDbContextManager();
            dbManagers.Add(x);
            return x;
        }

        public TestDataDbContextManager GetTestDataManager()
        {
            var x = new TestDataDbContextManager();
            dbManagers.Add(x);
            return x;
        }

        public void Dispose()
        {
            foreach (var x in dbManagers)
            {
                x.Remove();
            }
        }

        
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}