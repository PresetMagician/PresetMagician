using System.Data.Entity;

namespace PresetMagician.Migrations
{
    public interface IMigration
    {
        Database Database { get; set; }
        void Up();
    }
}