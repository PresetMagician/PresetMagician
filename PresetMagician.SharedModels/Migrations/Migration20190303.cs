namespace SharedModels.Migrations
{
	// ReSharper disable once UnusedMember.Global
	public class Migration20190303
        : BaseMigration
    {
        public override void Up()
        {
            if (ColumnExists("Presets", "IsDeleted"))
            {
	            Database.ExecuteSqlCommand(@"PRAGMA foreign_keys=0");
	            Database.ExecuteSqlCommand(@"DROP TABLE IF EXISTS tmp_20190303");
                Database.ExecuteSqlCommand(@"
CREATE TABLE 'tmp_20190303' (
				'PresetId'	nvarchar(128) NOT NULL,
				'PluginId'	int NOT NULL,
				'VstPluginId'	int NOT NULL,
				'LastExported'	datetime,
				'BankPath'	nvarchar,
				'PresetSize'	int NOT NULL,
				'PresetCompressedSize'	int NOT NULL,
				'PresetName'	nvarchar,
				'PreviewNoteNumber'	int NOT NULL,
				'Author'	nvarchar,
				'Comment'	nvarchar,
				'SourceFile'	nvarchar,
				'PresetHash'	nvarchar,
				'LastExportedPresetHash'	nvarchar,
				'IsIgnored'	bit NOT NULL DEFAULT 0,
				'IsMetadataModified'	bit NOT NULL DEFAULT 0,
				'UserModifiedMetadata'	nvarchar,
				PRIMARY KEY('PresetId'),
				FOREIGN KEY('PluginId') REFERENCES 'Plugins'('Id') ON DELETE CASCADE
					);

");
                // PRAGMA defer_foreign_keys = '1';
                Database.ExecuteSqlCommand(@"INSERT INTO tmp_20190303 SELECT PresetId,PluginId,VstPluginId,LastExported,BankPath,PresetSize,PresetCompressedSize,PresetName,PreviewNoteNumber,Author,Comment,SourceFile,PresetHash,LastExportedPresetHash,IsIgnored,IsMetadataModified,UserModifiedMetadata FROM Presets;
");
                Database.ExecuteSqlCommand(@"DROP TABLE Presets;");
                Database.ExecuteSqlCommand(@"ALTER TABLE tmp_20190303 RENAME TO Presets");
                Database.ExecuteSqlCommand(@"CREATE UNIQUE INDEX UniquePreset ON Presets (
				PluginId,
				SourceFile
					);");
            }
        }
    }
}