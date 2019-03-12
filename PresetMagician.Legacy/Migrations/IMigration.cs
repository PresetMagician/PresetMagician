using System.Data.Entity;

namespace PresetMagician.Legacy.Migrations
{
    public interface IMigration
    {
        Database Database { get; set; }
        void Up();
    }
}