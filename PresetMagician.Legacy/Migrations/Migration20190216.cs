namespace PresetMagician.Legacy.Migrations
{
    // ReSharper disable once UnusedMember.Global
    public class Migration20190216 : BaseMigration
    {
        public override void Up()
        {
            if (!ColumnExists("Presets", "UserModifiedMetadata"))
            {
                Database.ExecuteSqlCommand("ALTER TABLE `Presets` ADD COLUMN `UserModifiedMetadata` nvarchar");
            }
        }
    }
}