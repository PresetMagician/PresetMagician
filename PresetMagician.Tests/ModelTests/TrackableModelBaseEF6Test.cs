using System;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Catel.Linq;
using PresetMagician.Tests.TestEntities;
using SharedModels;
using SQLite.CodeFirst;
using Xunit;
using Xunit.Abstractions;

namespace PresetMagician.Tests.ModelTests
{
    public class TrackableModelBaseEF6Context : DbContext
    {
        public TrackableModelBaseEF6Context(string dbPath) : base(new SQLiteConnection(GetConnectionString(dbPath)), true)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ValidateOnSaveEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.AutoDetectChangesEnabled = false;
        }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer =
                new SqliteCreateDatabaseIfNotExists<TrackableModelBaseEF6Context>(modelBuilder);
           
            Database.SetInitializer(sqliteConnectionInitializer);
        }
        
        public static string GetConnectionString(string dbPath = null)
        {
            var cs = new SQLiteConnectionStringBuilder()
            {
                DataSource = dbPath, ForeignKeys = false, SyncMode = SynchronizationModes.Off,
                CacheSize = -10240
            };
            


            return cs.ConnectionString;
        }
        
        public DbSet<Company> Companies { get; set; }
        public DbSet<User> Users { get; set; }
    }
    public class TrackableModelBaseEF6Test
    {
        private readonly ITestOutputHelper output;
        
        public TrackableModelBaseEF6Test(ITestOutputHelper output)
        {
            this.output = output;
        }
        
        [Fact]
        public void TestTrackedCollectionSpeedWithUsers()
        {
            var dbPath = @"TestDatabases\" + Guid.NewGuid() + ".sqlite3";

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

            using (var context = new TrackableModelBaseEF6Context(dbPath))
            {
                context.Companies.Add(new Company());
                context.SaveChanges();
                
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                var company = new Company();
                for (int i = 0; i < 10000; i++)
                {
                    company.Users.Add(new User());
                }
                
                output.WriteLine($"Creating company and adding users: {stopWatch.ElapsedMilliseconds} ms");
                stopWatch.Restart();
                
                

                context.Companies.Add(company);
                context.SaveChanges();
                output.WriteLine($"Adding company todb: {stopWatch.ElapsedMilliseconds} ms");
                
                var company2 = new Company();
                context.Companies.Add(company2);
                for (int i = 0; i < 10000; i++)
                {
                    company2.Users.Add(new User());
                }
                context.SaveChanges();
                output.WriteLine($"Creating company, adding it to the context, and then adding users: {stopWatch.ElapsedMilliseconds} ms");
                stopWatch.Restart();
            }

            using (var context = new TrackableModelBaseEF6Context(dbPath))
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                var users = context.Users.ToList();
                var companies = context.Companies.ToList();
                output.WriteLine($"Loading company with 20k users: {stopWatch.ElapsedMilliseconds} ms");
            }
            
            using (var context = new TrackableModelBaseEF6Context(dbPath))
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                var companies = context.Companies.Include(p => p.Users).ToList();
                output.WriteLine($"Loading company with 20k users: {stopWatch.ElapsedMilliseconds} ms");
            }
            
         
            
           


        }
    }
}