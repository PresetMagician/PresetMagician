using System.Data.Entity;

namespace SharedModels.Migrations
{
    public interface IMigration
    {
        Database Database { get; set; }
        void Up();
    }
}