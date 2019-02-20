using System;
using System.Collections.Generic;
using System.IO;
using SharedModels;
using Path = Catel.IO.Path;

namespace PresetMagician.Tests
{
    public class TestDataDbContextManager: EmptyDbContextManager
    {
      
        public TestDataDbContextManager()
        {
            dbPath = @"TestDatabases\" + Guid.NewGuid() + ".sqlite3";

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            
            File.Copy(@"Resources\PresetMagician.test.sqlite3", dbPath);
            
        }

    }
}