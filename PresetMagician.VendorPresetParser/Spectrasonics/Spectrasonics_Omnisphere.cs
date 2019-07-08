using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using JetBrains.Annotations;
using MethodTimer;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Utils;
using PresetMagician.VendorPresetParser.Properties;
using Type = PresetMagician.Core.Models.Type;

namespace PresetMagician.VendorPresetParser.Spectrasonics
{
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class Spectrasonics_Omnisphere : AbstractVendorPresetParser, IVendorPresetParser
    {
        private const string LIBRARYTYPE_MULTIS = "Multis";
        private const string LIBRARYTYPE_PATCHES = "Patches";
        private const string FILESYSTEM_TAG = "FileSystem";
        private const string FILESYSTEM_START_TAG = "<" + FILESYSTEM_TAG + ">";
        private const string FILESYSTEM_END_TAG = "</" + FILESYSTEM_TAG + ">";
        private const string DIR_TAG = "DIR";
        private const string FILE_TAG = "FILE";
        private const string XML_TAG = "<?xml version=\"1.0\"?>";
        public const string PATCH_EXTENSION = ".prt_omn";
        public const string MULTI_EXTENSION = ".mlt_omn";
        public const string MULTI_INDEXFILE = "mlt_omn.index";
        public const string PATCH_INDEXFILE = "prt_omn.index";

        private readonly Dictionary<string, OmnisphereLibrary> _omnisphereLibraryCache =
            new Dictionary<string, OmnisphereLibrary>();

        private readonly Dictionary<string, OmnisphereUserLibrary> _omnisphereUserLibraryCache =
            new Dictionary<string, OmnisphereUserLibrary>();

        public override List<int> SupportedPlugins => new List<int> {1097687666};

        public override void Init()
        {
            BankLoadingNotes = $"STEAM Folder detected at {GetSteamFolder()}" + Environment.NewLine +
                               $"Library Folder detected at {GetLibraryFolder()}" + Environment.NewLine;
            base.Init();
        }

        public override async Task DoScan()
        {
            foreach (var library in GetLibraries())
            {
                try
                {
                    await DoLibraryScan(library);
                }
                catch (Exception e)
                {
                    Logger.Error($"Unable to scan library {library.Name} at {library.Path} because of {e.Message}");
                    Logger.Debug(e.StackTrace);
                }
            }

            await base.DoScan();
        }

        public override int GetNumPresets()
        {
            var count = 0;

            foreach (var library in GetLibraries())
            {
                try
                {
                    count += library.GetMultis().Count;
                }
                catch (Exception e)
                {
                    Logger.Error(
                        $"Unable to count multis for library {library.Name} at {library.Path} because of {e.Message}");
                    Logger.Debug(e.StackTrace);
                }

                try
                {
                    count += library.GetPatches().Count;
                }
                catch (Exception e)
                {
                    Logger.Error(
                        $"Unable to count patches for library {library.Name} at {library.Path} because of {e.Message}");
                    Logger.Debug(e.StackTrace);
                }
            }

            return base.GetNumPresets() + count;
        }

        public async Task DoLibraryScan(OmnisphereLibrary library)
        {
            library.BuildMetadata();
            foreach (var multi in library.GetMultis())
            {
                var presetData = library.GetFileContent(multi);
                var sourceFile = library.Path + "/" + multi.Directory.DirectoryPath + multi.Filename;


                var preset = new PresetParserMetadata
                {
                    PresetName = multi.FilenameWithoutExtension, Plugin = PluginInstance.Plugin,
                    BankPath = library.Name + "/" + multi.Directory.DirectoryPath,
                    SourceFile = sourceFile
                };

                ApplyMetadata(multi, preset);

                await DataPersistence.PersistPreset(preset, presetData);
            }

            foreach (var patch in library.GetPatches())
            {
                var presetData = Encoding.ASCII.GetString(library.GetFileContent(patch));

                var template = VendorResources.OmnispherePatchTemplate;
                presetData = presetData.Replace("<AmberPart >", "");
                presetData = presetData.Replace("<AmberPart>", "");
                presetData = presetData.Replace("</AmberPart>", "");

                template = template.Replace("{{PATCHNAME}}", patch.FilenameWithoutExtension);
                template = template.Replace("{{LIBRARYNAME}}", library.Name);
                template = template.Replace("{{PATCHGOESHERE}}", presetData);
                var sourceFile = library.Path + "/" + patch.Directory.DirectoryPath + patch.Filename;

                var preset = new PresetParserMetadata
                {
                    PresetName = patch.FilenameWithoutExtension, Plugin = PluginInstance.Plugin,
                    BankPath = library.Name + "/" + patch.Directory.DirectoryPath,
                    SourceFile = sourceFile
                };

                ApplyMetadata(patch, preset);

                await DataPersistence.PersistPreset(preset, Encoding.ASCII.GetBytes(template + "\0"));
            }
        }

        private void ApplyMetadata(FileSystemFile file, PresetParserMetadata metadata)
        {
            if (file.Attributes.ContainsKey("Author") && file.Attributes["Author"].Count > 0)
            {
                metadata.Author = string.Join(", ", file.Attributes["Author"]);
            }

            if (file.Attributes.ContainsKey("Description") && file.Attributes["Description"].Count > 0)
            {
                metadata.Comment = string.Join(Environment.NewLine, file.Attributes["Description"]);
            }

            if (file.Attributes.ContainsKey("Category"))
            {
                foreach (var category in file.Attributes["Category"])
                {
                    metadata.Types.Add(new Type {TypeName = category});
                }
            }

            if (file.Attributes.ContainsKey("Complexity"))
            {
                foreach (var category in file.Attributes["Complexity"])
                {
                    metadata.Characteristics.Add(new Characteristic {CharacteristicName = category});
                }
            }

            if (file.Attributes.ContainsKey("Gender"))
            {
                foreach (var category in file.Attributes["Gender"])
                {
                    metadata.Characteristics.Add(new Characteristic {CharacteristicName = category});
                }
            }

            if (file.Attributes.ContainsKey("Technique"))
            {
                foreach (var category in file.Attributes["Technique"])
                {
                    metadata.Characteristics.Add(new Characteristic {CharacteristicName = category});
                }
            }

            if (file.Attributes.ContainsKey("Type"))
            {
                foreach (var category in file.Attributes["Type"])
                {
                    metadata.Types.Add(new Type {TypeName = category});
                }
            }
        }

        private string GetSteamFolder()
        {
            var rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Spectrasonics", "STEAM");

            if (Directory.Exists(rootPath))
            {
                return rootPath;
            }

            rootPath += ".lnk";

            if (!ShortcutUtils.IsShortcut(rootPath))
            {
                throw new IOException("Could not find the default STEAM folder or shortcut");
            }

            return ShortcutUtils.ResolveShortcut(rootPath);
        }

        private string GetLibraryFolder(string type = null)
        {
            return type == null
                ? Path.Combine(GetSteamFolder(), "Omnisphere", "Settings Library")
                : Path.Combine(GetSteamFolder(), "Omnisphere", "Settings Library", type);
        }

        public List<OmnisphereLibrary> GetLibraries()
        {
            var libraries = new List<OmnisphereLibrary>();
            var files = GetCompressedLibraryFiles(LIBRARYTYPE_MULTIS);
            files.AddRange(GetCompressedLibraryFiles(LIBRARYTYPE_PATCHES));

            foreach (var file in files)
            {
                Logger.Info($"Parsing compressed library {file}");
                try
                {
                    var library = GetLibraryFilesystem(file);

                    if (library != null)
                    {
                        libraries.Add(library);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(
                        $"Unable to parse compressed library file {file}. Error: {e.GetType().FullName} {e.Message}");
                    Logger.LogException(e);
                }
            }


            foreach (var directory in GetPlainLibraryFiles(LIBRARYTYPE_MULTIS))
            {
                Logger.Info($"Parsing plain multi library file {directory}");
                libraries.Add(GetLibraryUser(Path.GetDirectoryName(directory)));
            }

            foreach (var directory in GetPlainLibraryFiles(LIBRARYTYPE_PATCHES))
            {
                Logger.Info($"Parsing plain patch library file {directory}");
                libraries.Add(GetLibraryUser(Path.GetDirectoryName(directory)));
            }

            return libraries;
        }

        public List<string> GetCompressedLibraryFiles(string type)
        {
            var libraryFolder = GetLibraryFolder(type);

            return Directory.EnumerateFiles(libraryFolder, "*.db", SearchOption.AllDirectories).ToList();
        }

        public List<string> GetPlainLibraryFiles(string type)
        {
            var libraryFolder = GetLibraryFolder(type);

            if (type == LIBRARYTYPE_MULTIS)
            {
                return Directory.EnumerateFiles(libraryFolder, MULTI_INDEXFILE, SearchOption.AllDirectories).ToList();
            }
            else
            {
                return Directory.EnumerateFiles(libraryFolder, PATCH_INDEXFILE, SearchOption.AllDirectories).ToList();
            }
        }

        public OmnisphereUserLibrary GetLibraryUser(string path)
        {
            if (_omnisphereUserLibraryCache.ContainsKey(path))
            {
                return _omnisphereUserLibraryCache[path];
            }

            var library = new OmnisphereUserLibrary();
            library.Path = path;

            var rootDirectory = new FileSystemDirectory();


            ParseUserDirectory(path, rootDirectory, library.Files);

            if (!_omnisphereUserLibraryCache.ContainsKey(path))
            {
                _omnisphereUserLibraryCache.Add(path, library);
            }

            return library;
        }

        private void ParseUserDirectory(string dirPath, FileSystemDirectory directory, List<FileSystemFile> entries)
        {
            foreach (var dir in Directory.EnumerateDirectories(dirPath))
            {
                var subDirectory = new FileSystemDirectory();

                subDirectory.DirectoryName = Path.GetFileName(dir);
                if (directory.DirectoryPath != null)
                {
                    subDirectory.DirectoryPath = directory.DirectoryPath + "/" + subDirectory.DirectoryName;
                }
                else
                {
                    subDirectory.DirectoryPath = subDirectory.DirectoryName;
                }


                ParseUserDirectory(Path.Combine(dirPath, subDirectory.DirectoryName), subDirectory, entries);
                subDirectory.ParentDirectory = directory;
                directory.Directories.Add(subDirectory);
            }

            foreach (var fileEntry in Directory.EnumerateFiles(dirPath, "*.*",
                SearchOption.TopDirectoryOnly))
            {
                var file = new FileSystemFile();
                file.Filename = Path.GetFileName(fileEntry);
                file.Directory = directory;

                entries.Add(file);
                directory.Files.Add(file);
            }
        }

        [Time]
        public OmnisphereLibrary GetLibraryFilesystem(string libraryFile)
        {
            if (_omnisphereLibraryCache.ContainsKey(libraryFile))
            {
                return _omnisphereLibraryCache[libraryFile];
            }


            var file = File.ReadAllText(libraryFile, Encoding.ASCII);
            var startFileSystemLocation = file.IndexOf(FILESYSTEM_START_TAG, StringComparison.OrdinalIgnoreCase);
            var endFileSystemLocation = file.IndexOf(FILESYSTEM_END_TAG, StringComparison.OrdinalIgnoreCase);

            if (startFileSystemLocation == -1)
            {
                Logger.Error("Unable to find the filesystem start tag.");
                return null;
            }

            if (startFileSystemLocation == -1)
            {
                Logger.Error("Unable to find the filesystem end tag");
                return null;
            }

            var fileSystemString = XML_TAG + Environment.NewLine + file.Substring(startFileSystemLocation,
                                       endFileSystemLocation - startFileSystemLocation + FILESYSTEM_END_TAG.Length);

            var fileSystemDocument = XDocument.Parse(fileSystemString);

            var library = new OmnisphereLibrary();
            library.Path = libraryFile;
            library.ContentOffset = endFileSystemLocation + FILESYSTEM_END_TAG.Length + 1;

            var rootElement = fileSystemDocument.Element(FILESYSTEM_TAG);

            var rootDirectory = new FileSystemDirectory();

            ParseFileSystemDirectory(rootElement, rootDirectory, library.Files);

            if (!_omnisphereLibraryCache.ContainsKey(libraryFile))
            {
                _omnisphereLibraryCache.Add(libraryFile, library);
            }

            return library;
        }

        private void ParseFileSystemDirectory(XElement rootElement, FileSystemDirectory directory,
            List<FileSystemFile> entries)
        {
            foreach (var dir in rootElement.Elements(DIR_TAG))
            {
                var subDirectory = new FileSystemDirectory();
                subDirectory.DirectoryName = dir.Attribute("name").Value;

                if (directory.DirectoryPath != null)
                {
                    subDirectory.DirectoryPath = directory.DirectoryPath + "/" + subDirectory.DirectoryName;
                }
                else
                {
                    subDirectory.DirectoryPath = subDirectory.DirectoryName;
                }

                subDirectory.ParentDirectory = directory;
                ParseFileSystemDirectory(dir, subDirectory, entries);
                directory.Directories.Add(subDirectory);
            }

            foreach (var file in rootElement.Elements(FILE_TAG))
            {
                var fileEntry = new FileSystemFile();
                fileEntry.Directory = directory;
                fileEntry.Filename = file.Attribute("name").Value;
                fileEntry.Offset = Convert.ToInt64(file.Attribute("offset").Value);
                fileEntry.Length = Convert.ToInt64(file.Attribute("size").Value);

                directory.Files.Add(fileEntry);
                entries.Add(fileEntry);
            }
        }
    }

    public class OmnisphereUserLibrary : OmnisphereLibrary
    {
        public override byte[] GetFileContent(FileSystemFile file)
        {
            var dir = file.Directory.DirectoryPath ?? "";
            return File.ReadAllBytes(System.IO.Path.Combine(Path, dir.Replace("/", "\\"),
                file.Filename));
        }

        public override FileSystemFile FindFile(string filename, string path)
        {
            foreach (var file in Files)
            {
                var dir = file.Directory.DirectoryPath ?? "";
                if (file.Filename == filename && dir == path)
                {
                    return file;
                }
            }

            return null;
        }
    }

    public class OmnisphereLibrary
    {
        public long ContentOffset;
        public List<FileSystemFile> Files = new List<FileSystemFile>();
        public List<string> MetadataTypes = new List<string>();
        public string Path;

        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

        public virtual byte[] GetFileContent(FileSystemFile file)
        {
            var buffer = new byte[file.Length];
            using (var fileStream = File.OpenRead(Path))
            {
                fileStream.Seek(ContentOffset + file.Offset, SeekOrigin.Begin);
                fileStream.Read(buffer, 0, (int) file.Length);
            }

            return buffer;
        }

        public virtual FileSystemFile FindFile(string filename, string path)
        {
            foreach (var file in Files)
            {
                if (file.Filename == filename && file.Directory.DirectoryPath == path)
                {
                    return file;
                }
            }

            return null;
        }

        [Time]
        public void BuildMetadata()
        {
            foreach (var file in Files)
            {
                if (file.Filename == Spectrasonics_Omnisphere.PATCH_INDEXFILE ||
                    file.Filename == Spectrasonics_Omnisphere.MULTI_INDEXFILE)
                {
                    var content = GetFileContent(file);
                    var data = Encoding.ASCII.GetString(content).Replace((char) 16, (char) 32);
                    var document = XDocument.Parse(data);

                    var extension = file.Filename == Spectrasonics_Omnisphere.PATCH_INDEXFILE
                        ? Spectrasonics_Omnisphere.PATCH_EXTENSION
                        : Spectrasonics_Omnisphere.MULTI_EXTENSION;

                    var indexElement = document.Element("INDEX");
                    if (indexElement.Attribute("VERSION")?.Value == "2")
                    {
                        ParseIndexEntries(document.Element("INDEX"), extension);
                    }
                    else
                    {
                        ParseIndexEntries(document.Element("INDEX"), extension, file.Directory.DirectoryPath);
                    }
                }
            }
        }

        public void ParseIndexEntries(XElement rootElement, string fileExtension, string overrideRootPath = null)
        {
            foreach (var entry in rootElement.Elements("ENTRY"))
            {
                FileSystemFile file;
                if (overrideRootPath != null)
                {
                    file = FindFile(entry.Attribute("NAME").Value + fileExtension,
                        overrideRootPath + entry.Attribute("PATH").Value);
                }
                else
                {
                    file = FindFile(entry.Attribute("NAME").Value + fileExtension, entry.Attribute("PATH").Value);
                }


                if (file == null)
                {
                    continue;
                }

                foreach (var attr in entry.Elements("ATTR"))
                {
                    var attributeType = attr.Attribute("NAME").Value;
                    var attributeValue = attr.Attribute("VALUE").Value;

                    if (!file.Attributes.ContainsKey(attributeType))
                    {
                        file.Attributes.Add(attributeType, new List<string>());
                    }

                    file.Attributes[attributeType].Add(attributeValue);

                    if (!MetadataTypes.Contains(attributeType))
                    {
                        MetadataTypes.Add(attributeType);
                    }
                }
            }
        }

        public List<FileSystemFile> GetPatches()
        {
            var patches = new List<FileSystemFile>();
            foreach (var file in Files)
            {
                if (file.Extension == Spectrasonics_Omnisphere.PATCH_EXTENSION)
                {
                    patches.Add(file);
                }
            }

            return patches;
        }

        public List<FileSystemFile> GetMultis()
        {
            var multis = new List<FileSystemFile>();
            foreach (var file in Files)
            {
                if (file.Extension == Spectrasonics_Omnisphere.MULTI_EXTENSION)
                {
                    multis.Add(file);
                }
            }

            return multis;
        }
    }

    public class FileSystemFile
    {
        public Dictionary<string, List<string>> Attributes = new Dictionary<string, List<string>>();
        public FileSystemDirectory Directory;
        public string Filename;
        public long Length;
        public long Offset;

        public string FilenameWithoutExtension => Filename.Replace(Spectrasonics_Omnisphere.MULTI_EXTENSION, "")
            .Replace(Spectrasonics_Omnisphere.PATCH_EXTENSION, "");

        public string Extension
        {
            get
            {
                var lastIndexOf = Filename.LastIndexOf(".", StringComparison.Ordinal);

                return lastIndexOf < 0 ? "" : Filename.Substring(lastIndexOf);
            }
        }

        public override string ToString()
        {
            return Directory.DirectoryPath + "/" + Filename;
        }
    }

    public class FileSystemDirectory
    {
        public List<FileSystemDirectory> Directories = new List<FileSystemDirectory>();
        public string DirectoryName;
        public string DirectoryPath;
        public List<FileSystemFile> Files = new List<FileSystemFile>();
        public FileSystemDirectory ParentDirectory;

        public override string ToString()
        {
            return DirectoryPath;
        }
    }
}