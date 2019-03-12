using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PresetMagician.Legacy.Migrations;
using SQLite.CodeFirst;
using PresetMagician.Legacy.Models;
using Type = PresetMagician.Legacy.Models.Type;

namespace PresetMagician.Legacy
{
    public class ApplicationDatabaseContext : DbContext
    {
        public static string OverrideDbPath;

        public static string DefaultDatabasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Drachenkatze\PresetMagician\PresetMagician.sqlite3");

      

        public ApplicationDatabaseContext() : base(new SQLiteConnection(GetConnectionString()), true)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ValidateOnSaveEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.AutoDetectChangesEnabled = false;
        }

        public ApplicationDatabaseContext(string overrideDbPath) : base(new SQLiteConnection(GetConnectionString(overrideDbPath)),
            true)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ValidateOnSaveEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.AutoDetectChangesEnabled = false;
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer =
                new SqliteCreateDatabaseIfNotExists<ApplicationDatabaseContext>(modelBuilder);

            modelBuilder.Entity<Plugin>().HasMany(p => p.DefaultModes).WithMany(q => q.Plugins).Map(mc =>
                mc.MapLeftKey("PluginId").MapRightKey("ModeId").ToTable("PluginModes"));
            modelBuilder.Entity<Plugin>().HasMany(p => p.DefaultTypes).WithMany(q => q.Plugins).Map(mc =>
                mc.MapLeftKey("PluginId").MapRightKey("TypeId").ToTable("PluginTypes"));
            modelBuilder.Entity<Plugin>();
            
            modelBuilder.Entity<Preset>().HasMany(p => p.Types).WithMany(q => q.Presets).Map(mc =>
                mc.MapLeftKey("PresetId").MapRightKey("TypeId").ToTable("PresetTypes"));

            modelBuilder.Entity<Preset>().HasMany(p => p.Modes).WithMany(q => q.Presets).Map(mc =>
                mc.MapLeftKey("PresetId").MapRightKey("ModeId").ToTable("PresetModes"));
            
            modelBuilder.Entity<Preset>();
            modelBuilder.Entity<Mode>();
            modelBuilder.Entity<Type>();
            modelBuilder.Entity<BankFile>();

            modelBuilder.Entity<PresetDataStorage>();
            modelBuilder.Entity<PluginLocation>();
            modelBuilder.Entity<SchemaVersion>();
            
            Database.SetInitializer(sqliteConnectionInitializer);
        }


        public static string GetDatabasePath(string dbPath = null)
        {
            if (!string.IsNullOrEmpty(dbPath))
            {
                return dbPath;
            }

            return DefaultDatabasePath;
        }

        public static string GetConnectionString(string dbPath = null)
        {
            var cs = new SQLiteConnectionStringBuilder()
            {
                DataSource = GetDatabasePath(dbPath), ForeignKeys = false, SyncMode = SynchronizationModes.Off,
                CacheSize = -10240
            };
            


            return cs.ConnectionString;
        }

        public static ApplicationDatabaseContext Create()
        {
            return new ApplicationDatabaseContext();
        }

        /// <summary>
        /// todo rename to initialize or something
        /// </summary>
        public static void InitializeViewCache()
        {
            List<System.Type> Models = new List<System.Type>
            {
                typeof(Plugin),
                typeof(Preset),
                typeof(Mode),
                typeof(Type),
                typeof(PresetDataStorage),
                typeof(PluginLocation),
                typeof(BankFile)
            };
            
            using (var context = new ApplicationDatabaseContext())
            {
                context.Migrate();
            }
        }

      

        public byte[] GetPresetData(Preset preset)
        {
            
                var data = PresetDataStorage.Find(preset.PresetId);
                if (data != null)
                {
                    return data.PresetData;
                }
                
                return null;
            
        }


        public void Migrate()
        {
            var type = typeof(IMigration);

            SchemaVersions.Load();
            var currentAssembly = Assembly.GetExecutingAssembly();

            var types = currentAssembly.GetTypes()
                .Where(p => p.GetInterfaces().Contains(type)).OrderBy(p => p.Name);

            foreach (var migration in types)
            {
                if (migration.IsAbstract)
                {
                    continue;
                }

                var migrationExecuted = (from schemaVersion in SchemaVersions
                    where schemaVersion.Version == migration.Name
                    select schemaVersion).Any();

                if (migrationExecuted)
                {
                    continue;
                }

                BaseMigration instance = (BaseMigration) Activator.CreateInstance(migration);
                instance.Database = Database;
                instance.Up();

                var executedMigration = new SchemaVersion();
                executedMigration.Version = migration.Name;
                SchemaVersions.Add(executedMigration);
            }

            SaveChanges();
        }


        public DbSet<Plugin> Plugins { get; set; }
        public DbSet<Preset> Presets { get; set; }

        public DbSet<PluginLocation> PluginLocations { get; set; }

        public DbSet<Type> Types { get; set; }

        public DbSet<Mode> Modes { get; set; }
        public DbSet<SchemaVersion> SchemaVersions { get; set; }
        public DbSet<PresetDataStorage> PresetDataStorage { get; set; }
    }

}