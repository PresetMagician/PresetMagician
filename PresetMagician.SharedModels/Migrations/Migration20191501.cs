using System.Diagnostics;

namespace PresetMagician.Migrations
{
    public class Migration20191501: BaseMigration
    {
        public override void Up()
        {
            if (!ColumnExists("Presets", "LastExported"))
            {
                Database.ExecuteSqlCommand("ALTER TABLE `Presets` ADD COLUMN `LastExported` DATETIME");
            }

        }
    }
}