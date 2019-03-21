namespace PresetMagician.Legacy.Migrations
{
    // ReSharper disable once UnusedMember.Global
    public class Migration20190215 : BaseMigration
    {
        public override void Up()
        {
            if (!ColumnExists("Presets", "IsIgnored"))
            {
                Database.ExecuteSqlCommand("ALTER TABLE `Presets` ADD COLUMN `IsIgnored` bit not null DEFAULT 0");
            }

            if (!ColumnExists("Presets", "IsMetadataModified"))
            {
                Database.ExecuteSqlCommand(
                    "ALTER TABLE `Presets` ADD COLUMN `IsMetadataModified` bit not null DEFAULT 0");
            }
        }
    }
}