namespace PresetMagician.Migrations
{
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