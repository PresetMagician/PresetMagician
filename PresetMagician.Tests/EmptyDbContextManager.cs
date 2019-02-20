using System;
using System.Collections.Generic;
using System.IO;
using SharedModels;
using Path = Catel.IO.Path;

namespace PresetMagician.Tests
{
    public class EmptyDbContextManager
    {
        protected string dbPath;
        
        public EmptyDbContextManager()
        {
            dbPath = @"TestDatabases\" + Guid.NewGuid() + ".sqlite3";

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            
        }

        public string GetDbPath()
        {
            return dbPath;
        }

        public ApplicationDatabaseContext Create()
        {
            
            return new ApplicationDatabaseContext(dbPath);
        }

        public void Remove()
        {
           /* GC.Collect(); GC.WaitForPendingFinalizers();
             File.Delete(dbPath);*/
            
        }
    }
}