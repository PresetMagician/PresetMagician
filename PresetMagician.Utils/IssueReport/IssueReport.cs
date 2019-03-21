using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Catel.Data;
using Drachenkatze.PresetMagician.Utils.Progress;
using Redmine.Net.Api;
using Redmine.Net.Api.Async;
using Redmine.Net.Api.Types;
using SQLite;
using SystemFile = System.IO.File;
using Version = Redmine.Net.Api.Types.Version;

namespace PresetMagician.Utils.IssueReport
{
    public class IssueAttachment
    {
        public string Filename => Path.GetFileName(FilePath);

        public string Description { get; set; }
        public bool DeleteAfterReport { get; set; }
        public string FilePath { get; set; }
    }

    public class IssueReport : ValidatableModelBase
    {
        private const string RedmineHost = "https://support.presetmagician.com";
        private const string RedmineApiKey = "3c276fc5c309a9aa4d07486210f8d042ebb33cea";

        private const int ProjectIdSupportConfidential = 1;
        private const int ProjectIdGeneral = 2;
        private const int ProjectIdVendor = 3;
        private const string ProjectNameGeneral = "presetmagician";

        private const int TrackerIdBug = 1;
        private const int TrackerIdFeature = 2;
        private const int TrackerIdCrash = 5;
        private const int TrackerIdSupport = 3;

        public enum TrackerTypes
        {
            BUG,
            FEATURE,
            SUPPORT,
            CRASH
        }

        private TrackerTypes _trackerType;

        public TrackerTypes TrackerType
        {
            get => _trackerType;
            set
            {
                _trackerType = value;

                if (_trackerType == TrackerTypes.CRASH || _trackerType == TrackerTypes.SUPPORT)
                {
                    SubmitPrivately = true;
                }
            }
        }

        [Required] [MinLength(10)] public string Subject { get; set; }
        public string Description { get; set; }
        private string Version { get; }
        [Required] [EmailAddress] public string UserEmail { get; set; }
        public bool SubmitPrivately { get; set; }
        public bool IncludeDatabase { get; set; }
        public bool IncludeSystemLog { get; set; }
        public bool IncludePluginLog { get; set; }

        private string SystemLogLocation { get; }
        private string DatabaseLocation { get; }
        public string PluginId { get; set; }
        public string PluginLog { get; set; }
        public string PluginName { get; set; }
        public string PluginVendor { get; set; }
        public string PluginVstId { get; set; }
        public bool RequiresEmail { get; }
        private Issue _issue;
        private RedmineManager _manager;

        public ObservableCollection<IssueAttachment> Attachments { get; } = new ObservableCollection<IssueAttachment>();

        public IssueReport(TrackerTypes trackerType, string version, string userEmail, string systemLogLocation,
            string databaseLocation)
        {
            TrackerType = trackerType;
            Version = version;
            UserEmail = userEmail;

            if (string.IsNullOrWhiteSpace(UserEmail))
            {
                RequiresEmail = true;
            }

            DatabaseLocation = databaseLocation;
            SystemLogLocation = systemLogLocation;
        }

        public void SetException(Exception e)
        {
            var tmpFile = Path.GetTempPath() + Guid.NewGuid() + ".txt";

            SystemFile.WriteAllText(tmpFile, e.ToString());
            Attachments.Add(new IssueAttachment
                {FilePath = tmpFile, Description = "Exception Information", DeleteAfterReport = true});

            Subject = $"Crash caused by: {e.GetType().FullName}: {e.Message}";
        }

        public void CreatePluginLog()
        {
            var tmpFile = Path.GetTempPath() + Guid.NewGuid() + ".txt";

            SystemFile.WriteAllText(tmpFile, PluginLog);
            Attachments.Add(new IssueAttachment
                {FilePath = tmpFile, Description = "Plugin Log", DeleteAfterReport = true});
        }


        public void AddSystemLog()
        {
            Attachments.Add(new IssueAttachment
                {FilePath = SystemLogLocation, Description = "PresetMagician Log", DeleteAfterReport = false});
        }

        public async Task PrepareIssue(IProgress<StringProgress> progress)
        {
            progress.Report(new StringProgress("Retrieving/Creating Version Number"));
            _manager = new RedmineManager(RedmineHost, RedmineApiKey);

            _manager.ImpersonateUser = await GetOrCreateUserToImpersonate(UserEmail);

            var version = await GetOrCreateVersion(Version);

            var versionCustomField = new IssueCustomField
            {
                Id = 1,
                Name = "Occurred in",
                Values = new List<CustomFieldValue>()
            };

            versionCustomField.Values.Add(new CustomFieldValue {Info = version.Id.ToString()});

            if (IncludeDatabase)
            {
                await CreateStrippedDatabaseCopy(PluginId, progress);
            }

            if (IncludePluginLog)
            {
                CreatePluginLog();
            }

            if (IncludeSystemLog)
            {
                AddSystemLog();
            }

            var attachmentUploads = await UploadAttachments(Attachments, progress);
            _issue = new Issue();
            _issue.CustomFields = new List<IssueCustomField>();

            switch (TrackerType)
            {
                case TrackerTypes.BUG:
                    _issue.Tracker = new IdentifiableName {Id = TrackerIdBug};
                    _issue.Project = new IdentifiableName {Id = ProjectIdGeneral};
                    break;
                case TrackerTypes.CRASH:
                    _issue.Tracker = new IdentifiableName {Id = TrackerIdCrash};
                    _issue.Project = new IdentifiableName {Id = ProjectIdSupportConfidential};
                    break;
                case TrackerTypes.SUPPORT:
                    _issue.Tracker = new IdentifiableName {Id = TrackerIdSupport};
                    _issue.Project = new IdentifiableName {Id = ProjectIdSupportConfidential};
                    break;
                case TrackerTypes.FEATURE:
                    _issue.Tracker = new IdentifiableName {Id = TrackerIdFeature};
                    _issue.Project = new IdentifiableName {Id = ProjectIdGeneral};
                    break;
            }

            if (PluginId != null)
            {
                _issue.Project = new IdentifiableName {Id = ProjectIdVendor};

                var pluginName = new IssueCustomField
                {
                    Id = 2,
                    Name = "Plugin Name",
                    Values = new List<CustomFieldValue>()
                };

                pluginName.Values.Add(new CustomFieldValue {Info = PluginName});

                var pluginVendor = new IssueCustomField
                {
                    Id = 3,
                    Name = "Plugin Vendor",
                    Values = new List<CustomFieldValue>()
                };

                pluginVendor.Values.Add(new CustomFieldValue {Info = PluginVendor});

                var pluginVstId = new IssueCustomField
                {
                    Id = 4,
                    Name = "Plugin VST Id",
                    Values = new List<CustomFieldValue>()
                };

                pluginVstId.Values.Add(new CustomFieldValue {Info = PluginVstId});

                _issue.CustomFields.Add(pluginName);
                _issue.CustomFields.Add(pluginVendor);
                _issue.CustomFields.Add(pluginVstId);
            }

            _issue.IsPrivate = SubmitPrivately;
            _issue.CustomFields.Add(versionCustomField);
            _issue.Subject = Subject;
            _issue.Description = Description;
            _issue.Uploads = attachmentUploads;
        }

        public async Task SubmitIssue()
        {
            await _manager.CreateObjectAsync(_issue);

            foreach (var attachment in Attachments)
            {
                if (attachment.DeleteAfterReport)
                {
                    SystemFile.Delete(attachment.FilePath);
                }
            }
        }

        private async Task<List<Upload>> UploadAttachments(
            IEnumerable<IssueAttachment> attachmentFiles, IProgress<StringProgress> progress)
        {
            var attachments = new List<Upload>();

            foreach (var file in attachmentFiles)
            {
                if (!SystemFile.Exists(file.FilePath))
                {
                    continue;
                }

                progress.Report(new StringProgress($"Uploading {file.Filename}"));
                var documentData = SystemFile.ReadAllBytes(file.FilePath);

                var attachment = await _manager.UploadFileAsync(documentData);

                //set attachment properties
                attachment.FileName = Path.GetFileName(file.FilePath);

                attachment.Description = file.Description;
                attachment.ContentType = MimeTypeMap.List.MimeTypeMap.GetMimeType(Path.GetExtension(file.FilePath))
                    .FirstOrDefault();

                if (attachment.ContentType == null)
                {
                    attachment.ContentType = "application/octet-stream";
                }

                attachments.Add(attachment);
            }

            return attachments;
        }


        private async Task<Version> GetOrCreateVersion(string versionNumber)
        {
            var versionParameters = new NameValueCollection
            {
                {RedmineKeys.STATUS_ID, RedmineKeys.ALL}, {RedmineKeys.PROJECT_ID, ProjectIdGeneral.ToString()}
            };
            var versions = await _manager.GetObjectsAsync<Version>(versionParameters);

            Version foundVersion = null;

            foreach (var version in versions)
            {
                if (version.Name == versionNumber)
                {
                    foundVersion = version;
                }
            }

            if (foundVersion != null)
            {
                return foundVersion;
            }

            var versionToCreate = new Version();
            versionToCreate.Name = versionNumber;
            versionToCreate.Sharing = VersionSharing.system;
            versionToCreate.Status = VersionStatus.open;

            foundVersion = await _manager.CreateObjectAsync(versionToCreate, ProjectNameGeneral);

            return foundVersion;
        }


        private async Task<string> GetOrCreateUserToImpersonate(string userMail)
        {
            var userParameters = new NameValueCollection
            {
                {RedmineKeys.STATUS_ID, RedmineKeys.ALL}, {RedmineKeys.NAME, userMail}
            };

            //parameter - fetch issues for a date range

            string impersonateUser = null;
            var foundUsers = await _manager.GetObjectsAsync<User>(userParameters);

            foreach (var user in foundUsers)
            {
                if (user.Email == userMail)
                {
                    impersonateUser = user.Login;
                }
            }

            if (impersonateUser == null)
            {
                // Create new user
                var user = new User
                {
                    Email = userMail,
                    MustChangePassword = true,
                    LastName = "from PresetMagician",
                    FirstName = "Reporter",
                    Login = Guid.NewGuid().ToString()
                };
                var createdUser = await _manager.CreateObjectAsync(user);

                await _manager.AddUserToGroupAsync(6, createdUser.Id);
                impersonateUser = createdUser.Login;
            }

            return impersonateUser;
        }

        public async Task CreateStrippedDatabaseCopy(string restrictToPlugin, IProgress<StringProgress> progress)
        {
            throw new Exception("Not implemented");
            var tempDatabasePath = Path.Combine(Path.GetTempPath(), "PresetMagician.Stripped.sqlite3");
            var tempDatabaseZip = tempDatabasePath + ".zip";
            if (SystemFile.Exists(tempDatabasePath))
            {
                SystemFile.Delete(tempDatabasePath);
            }

            progress.Report(new StringProgress("Copying database to a temporary location"));
            SystemFile.Copy(DatabaseLocation, tempDatabasePath);

            var db = new SQLiteAsyncConnection(tempDatabasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
            await db.ExecuteAsync("PRAGMA foreign_keys = OFF;");

            progress.Report(new StringProgress("Cleaning/stripping temporary database"));
            if (restrictToPlugin != null)
            {
                await db.ExecuteAsync("DELETE FROM Plugins WHERE Id != ?", restrictToPlugin);
                await db.ExecuteAsync("delete from Presets WHERE Presets.PluginId != ?", restrictToPlugin);
            }

            await db.ExecuteAsync(
                "DELETE FROM Presets where Presets._rowid_ not in (select t2._rowid_ from Presets t2 where t2.PluginId = Presets.PluginId limit 10)");
            await db.ExecuteAsync(
                "DELETE FROM PresetDataStorages where PresetDataStorages.PresetDataStorageId not in (select PresetId from Presets)");

            if (restrictToPlugin == null)
            {
                await db.ExecuteAsync("UPDATE PresetDataStorages SET PresetData='', CompressedPresetData='';");
            }

            await db.ExecuteAsync("VACUUM;");
            await db.CloseAsync();

            // todo Workaround for issue https://github.com/praeclarum/sqlite-net/issues/762
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (SystemFile.Exists(tempDatabaseZip))
            {
                SystemFile.Delete(tempDatabaseZip);
            }

            progress.Report(new StringProgress("Compressing database"));
            using (var zip = ZipFile.Open(tempDatabaseZip, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(tempDatabasePath, Path.GetFileName(tempDatabasePath));
            }

            SystemFile.Delete(tempDatabasePath);

            Attachments.Add(new IssueAttachment
                {FilePath = tempDatabaseZip, Description = "Stripped Database", DeleteAfterReport = true});
        }
    }
}