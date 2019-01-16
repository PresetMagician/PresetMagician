using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using Catel.Linq;

namespace PresetMagician.Migrations
{
    abstract public class BaseMigration : IMigration
    {
        public Database Database { get; set; }
        public abstract void Up();

        // This method will check if column exists in your table
        public bool ColumnExists(string tableName, string fieldName)
        {
            var results =
                Database.SqlQuery<TablePragmaResults>(
                    "PRAGMA table_info("+tableName+")").ToArray();

            foreach (var result in results)
            {
                if (result.name == fieldName)
                {
                    return true;
                }
            }

            return false;
        }
        
        internal class TablePragmaResults
        {
            public int cid { get; set; }
            public string name { get; set; }
        }
    }
}