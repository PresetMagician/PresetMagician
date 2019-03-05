using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Catel.Linq;
using FluentAssertions;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using PresetMagician.Tests.TestEntities;
using SharedModels;
using SQLite.CodeFirst;
using Xunit;
using Xunit.Abstractions;
using Type = System.Type;

namespace PresetMagician.Tests.ModelTests
{
    class ExampleAutomappingConfiguration : DefaultAutomappingConfiguration
    {
        public override bool ShouldMap(Type type)
        {
            // specify the criteria that types must meet in order to be mapped
            // any type for which this method returns false will not be mapped.
            Debug.WriteLine(type.FullName);
            return type.Namespace == "PresetMagician.Tests.TestEntities";
        }

        public override bool IsComponent(Type type)
        {
            // override this method to specify which types should be treated as components
            // if you have a large list of types, you should consider maintaining a list of them
            // somewhere or using some form of conventional and/or attribute design
            return false;//return type == typeof(User);
        }
        
        
    }
    
    class CascadeConvention : IReferenceConvention, IHasManyConvention, IHasManyToManyConvention
    {
        public void Apply(IManyToOneInstance instance)
        {
            instance.Cascade.All();
        }

        public void Apply(IOneToManyCollectionInstance instance)
        {
            instance.Cascade.All();
        }

        public void Apply(IManyToManyCollectionInstance instance)
        {
            instance.Cascade.All();
        }
    }
    
    public class TrackableModelBaseHibernateTest
    {
        
        private readonly ITestOutputHelper output;
        
        public TrackableModelBaseHibernateTest(ITestOutputHelper output)
        {
            this.output = output;
        }
                static AutoPersistenceModel CreateAutomappings()
        {
            // This is the actual automapping - use AutoMap to start automapping,
            // then pick one of the static methods to specify what to map (in this case
            // all the classes in the assembly that contains Employee), and then either
            // use the Setup and Where methods to restrict that behaviour, or (preferably)
            // supply a configuration instance of your definition to control the automapper.
            return AutoMap.AssemblyOf<Company>(new ExampleAutomappingConfiguration())
                .Conventions.Add<CascadeConvention>()
                
                .OverrideAll(map => { map.IgnoreProperties("PropertyChanged",
                    "TrackingState", "EditableProperties", "ModifiedProperties", "IsUserModified", "EntityIdentifier","UserModifiedProperties", "IsEditing"); });
        }

        /// <summary>
        /// Configure NHibernate. This method returns an ISessionFactory instance that is
        /// populated with mappings created by Fluent NHibernate.
        /// 
        /// Line 1:   Begin configuration
        ///      2+3: Configure the database being used (SQLite file db)
        ///      4+5: Specify what mappings are going to be used (Automappings from the CreateAutomappings method)
        ///      6:   Expose the underlying configuration instance to the BuildSchema method,
        ///           this creates the database.
        ///      7:   Finally, build the session factory.
        /// </summary>
        /// <returns></returns>
        private static ISessionFactory CreateSessionFactory(string dbFile)
        {
            return Fluently.Configure()
                .Database(SQLiteConfiguration.Standard
                    .UsingFile(dbFile))
                .Mappings(m =>
                    m.AutoMappings.Add(CreateAutomappings))
                .ExposeConfiguration(BuildSchema)
                .BuildSessionFactory();
        }

        private static void BuildSchema(Configuration config)
        {

            // this NHibernate tool takes a configuration (with mapping info in)
            // and exports a database schema from it
            new SchemaExport(config)
                .Create(false, true);
        }
        
        [Fact]
        public void TestTrackedCollectionSpeedWithUsers()
        {
            var dbPath = @"TestDatabases\" + Guid.NewGuid() + ".sqlite3";

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
            
            var sessionFactory = CreateSessionFactory(dbPath);

        
            
            using (var session = sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    var company = new Company();
                    for (int i = 0; i < 10000; i++)
                    {
                        company.Users.Add(new User());
                    }

                    
                    session.SaveOrUpdate(company);
                    transaction.Commit();
                    
                    output.WriteLine($"Creating company and adding users: {stopWatch.ElapsedMilliseconds} ms");
                    stopWatch.Restart();
                }
            }

            IList<Company> foo;
            using (var session = sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                { Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    foo = session.Query<Company>().FetchMany(p => p.Users).ToList();
                   
                  
                    
                    output.WriteLine($"Loading the company and users: {stopWatch.ElapsedMilliseconds} ms");
                    stopWatch.Restart();
                }
            }

            foo.First().Users.Count.Should().BeGreaterThan(5000);












        }
    }
}