using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using SQLite.CodeFirst;

namespace PresetMagicianScratchPad
{
    public class Mode
    {
        [Key] public int Id { get; set; }

        public ICollection<Plugin> Plugins { get; set; }
        public ICollection<Preset> Presets { get; set; }

        public string Name { get; set; }
    }

    public class Type
    {
        [Key] public int Id { get; set; }

        public ICollection<Plugin> Plugins { get; set; }
        public ICollection<Preset> Presets { get; set; }

        public string Name { get; set; }
        public string SubTypeName { get; set; }
    }

    public class Plugin
    {
        [Key] public int Id { get; set; }
        public ICollection<Preset> Presets { get; set; }

        public ICollection<Type> DefaultTypes { get; set; }
        public ICollection<Mode> DefaultModes { get; set; }
    }

    public class Preset
    {
        [Key] public int Id { get; set; }
        public Plugin Plugin { get; set; }
        public ICollection<Type> Types { get; set; }
        public ICollection<Mode> Modes { get; set; }
    }

    public class ApplicationDatabaseContext : DbContext
    {
        public ApplicationDatabaseContext() : base(new SQLiteConnection(GetConnectionString()), true)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ValidateOnSaveEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        public static string GetConnectionString()
        {
            var cs = new SQLiteConnectionStringBuilder()
            {
                DataSource = "foo.sqlite3", ForeignKeys = false, SyncMode = SynchronizationModes.Off,
                CacheSize = -10240
            };


            return cs.ConnectionString;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer =
                new SqliteCreateDatabaseIfNotExists<ApplicationDatabaseContext>(modelBuilder);
            modelBuilder.Entity<Plugin>().HasMany(p => p.DefaultModes).WithMany(q => q.Plugins).Map(mc =>
                mc.MapLeftKey("PluginId").MapRightKey("ModeId").ToTable("PluginModes"));

            modelBuilder.Entity<Plugin>().HasMany(p => p.DefaultTypes).WithMany(q => q.Plugins).Map(mc =>
                mc.MapLeftKey("PluginId").MapRightKey("TypeId").ToTable("PluginTypes"));


            modelBuilder.Entity<Preset>().HasMany(p => p.Types).WithMany(q => q.Presets).Map(mc =>
                mc.MapLeftKey("PresetId").MapRightKey("TypeId").ToTable("PresetTypes"));

            modelBuilder.Entity<Preset>().HasMany(p => p.Modes).WithMany(q => q.Presets).Map(mc =>
                mc.MapLeftKey("PresetId").MapRightKey("ModeId").ToTable("PresetModes"));
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        public DbSet<Plugin> Plugins { get; set; }
        
    }


    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var context = new ApplicationDatabaseContext())
            {
                context.Plugins
                    .Include("Presets.Modes")
                    .Include("Presets.Types")
                    .AsNoTracking().ToList();
            }
            
        }
    }
}