namespace PresetMagician.Migrations
{
    public class Migration20191501 : BaseMigration
    {
        public override void Up()
        {
            if (!ColumnExists("Presets", "IsIgnored"))
            {
                Database.ExecuteSqlCommand("ALTER TABLE `Presets` ADD COLUMN `IsIgnored` bit not null DEFAULT 0");
            }

            if (!ColumnExists("Presets", "IsMetadataModified"))
            {
                Database.ExecuteSqlCommand("ALTER TABLE `Presets` ADD COLUMN `IsMetadataModified` bit not null DEFAULT 0");
            }
        }
    }
}