using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Anotar.Catel;
using Catel.Data;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PresetMagician.Extensions;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace PresetMagician.Models.NativeInstrumentsResources
{
    public class NativeInstrumentsResource : ModelBase
    {
        public Color Color { get; set; } = new Color();
        public Categories Categories { get; set; } = new Categories();
        public ShortNames ShortNames { get; set; } = new ShortNames();

        #region Images

        /*public string MST_artwork { get; set; }
        public string MST_logo { get; set; }
        public string MST_plugin { get; set; }
        
        public string OSO_logo { get; set; }
        public string VB_artwork { get; set; }
        public string VB_logo { get; set; }*/

        #endregion

        public BitmapImage VB_logo { get; set; } = new BitmapImage();
        public MemoryStream VB_logoStream { get; set; } = new MemoryStream();

        public BitmapImage VB_artwork { get; set; } = new BitmapImage();
        public MemoryStream VB_artworkStream { get; set; } = new MemoryStream();

        public BitmapImage MST_artwork { get; set; } = new BitmapImage();
        public MemoryStream MST_artworkStream { get; set; } = new MemoryStream();

        public BitmapImage MST_plugin { get; set; } = new BitmapImage();
        public MemoryStream MST_pluginStream { get; set; } = new MemoryStream();

        public BitmapImage MST_logo { get; set; } = new BitmapImage();
        public MemoryStream MST_logoStream { get; set; } = new MemoryStream();

        public BitmapImage OSO_logo { get; set; } = new BitmapImage();
        public MemoryStream OSO_logoStream { get; set; } = new MemoryStream();

        public static string GetNativeInstrumentsResourcesDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                "NI Resources");
        }

        public static string GetDistDatabaseDirectory(IVstPlugin plugin)
        {
            return Path.Combine(GetNativeInstrumentsResourcesDirectory(), "dist_database",
                plugin.PluginVendor.ToLower(), plugin.PluginName.ToLower());
        }

        public static string GetImageDirectory(IVstPlugin plugin)
        {
            return Path.Combine(GetNativeInstrumentsResourcesDirectory(), "image", plugin.PluginVendor.ToLower(),
                plugin.PluginName.ToLower());
        }

        public void Save(IVstPlugin plugin)
        {
            var files = GetFiles(plugin);

            var ResourcesDirectory = GetDistDatabaseDirectory(plugin);
            var ImagesDirectory = GetImageDirectory(plugin);

            if (!Directory.Exists(ResourcesDirectory))
            {
                Directory.CreateDirectory(ResourcesDirectory);
            }

            if (!Directory.Exists(ImagesDirectory))
            {
                Directory.CreateDirectory(ImagesDirectory);
            }

            Color.VB_bgcolor = $"{Color.BackgroundColor.R:X2}{Color.BackgroundColor.G:X2}{Color.BackgroundColor.B:X2}";
            File.WriteAllText(files["color"], JsonConvert.SerializeObject(Color));

            File.WriteAllText(files["shortname"], JsonConvert.SerializeObject(ShortNames));

            try
            {
                Categories.CategoryDB.First().Categories.Clear();

                foreach (var category in Categories.CategoryNames)
                {
                    Categories.CategoryDB.First().Categories.Add(category.Name);
                }

                File.WriteAllText(files["categories"], JsonConvert.SerializeObject(Categories));
            }
            catch (Exception e)
            {
                LogTo.Error($"Error occured while saving categories: {e.Message}");
                LogTo.Debug(e.StackTrace);
            }

            try
            {
                var metaFile = Path.Combine(ResourcesDirectory, plugin.PluginName.ToLower() + ".meta");
                if (!File.Exists(metaFile))
                {
                    CreateMetaFile(plugin, "dist_database", metaFile);
                }
                
                var imageMetaFile = Path.Combine(ImagesDirectory, plugin.PluginName.ToLower() + ".meta");
                if (!File.Exists(imageMetaFile))
                {
                    CreateMetaFile(plugin, "image", imageMetaFile);
                }
            }
            catch (Exception e)
            {
                LogTo.Error($"Error occured while saving metadata: {e.Message}");
                LogTo.Debug(e.StackTrace);
            }

            if (VB_logoStream.Length > 0)
            {
                File.WriteAllBytes(files["VB_logo"], VB_logoStream.ToArray());
            }
            
            if (VB_artworkStream.Length > 0)
            {
                File.WriteAllBytes(files["VB_artwork"], VB_artworkStream.ToArray());
            }
            
            if (MST_logoStream.Length > 0)
            {
                File.WriteAllBytes(files["MST_logo"], MST_logoStream.ToArray());
            }
            
            if (MST_artworkStream.Length > 0)
            {
                File.WriteAllBytes(files["MST_artwork"], MST_artworkStream.ToArray());
            }
            
            if (MST_pluginStream.Length > 0)
            {
                File.WriteAllBytes(files["MST_plugin"], MST_pluginStream.ToArray());
            }
            
            if (OSO_logoStream.Length > 0)
            {
                File.WriteAllBytes(files["OSO_logo"], OSO_logoStream.ToArray());
            }
        }

        public void CreateMetaFile(IVstPlugin plugin, string dbType, string outputFile)
        {
            var doc = new XDocument();
            doc.Declaration = new XDeclaration("1.0", "UTF-8", "no");

            var resource = new XElement("resource");
            resource.SetAttributeValue("version", "1.0");

            var vendor = new XElement("vendor");
            vendor.Value = plugin.PluginVendor;
            resource.Add(vendor);

            var name = new XElement("name");
            name.Value = plugin.PluginName;
            resource.Add(name);

            var type = new XElement("type");
            type.Value = dbType;
            resource.Add(type);

            doc.Add(resource);

            doc.Save(outputFile);
        }

        public Dictionary<string, string> GetFiles(IVstPlugin plugin)
        {
            Dictionary<string, string> files = new Dictionary<string, string>();
            var ResourcesDirectory = GetDistDatabaseDirectory(plugin);
            var ImagesDirectory = GetImageDirectory(plugin);

            files.Add("color", Path.Combine(ResourcesDirectory, "color.json"));
            files.Add("shortname", Path.Combine(ResourcesDirectory, "shortname.json"));
            files.Add("categories", Path.Combine(ResourcesDirectory, "categories.json"));

            files.Add("VB_logo", Path.Combine(ImagesDirectory, "VB_logo.png"));
            files.Add("VB_artwork", Path.Combine(ImagesDirectory, "VB_artwork.png"));
            files.Add("MST_logo", Path.Combine(ImagesDirectory, "MST_logo.png"));
            files.Add("MST_artwork", Path.Combine(ImagesDirectory, "MST_artwork.png"));
            files.Add("MST_plugin", Path.Combine(ImagesDirectory, "MST_plugin.png"));
            files.Add("OSO_logo", Path.Combine(ImagesDirectory, "OSO_logo.png"));

            return files;
        }

        public void LoadFromJObject(JObject obj)
        {
            Color.BackgroundColor =
                (System.Windows.Media.Color) ColorConverter.ConvertFromString("#" + obj["bgColor"]);
            
            ShortNames.VB_shortname = (string)obj["shortName_VB"];
            ShortNames.MKII_shortname = (string)obj["shortName_MKII"];
            ShortNames.MST_shortname = (string)obj["shortName_MST"];
            ShortNames.MIKRO_shortname = (string)obj["shortName_MIKRO"];

            VB_logo = ReplaceImageFromBase64((string) obj["image_VB_logo"], VB_logoStream);
            VB_artwork = ReplaceImageFromBase64((string) obj["image_VB_artwork"], VB_artworkStream);
            MST_logo = ReplaceImageFromBase64((string) obj["image_MST_logo"], MST_logoStream);
            MST_artwork = ReplaceImageFromBase64((string) obj["image_MST_artwork"], MST_artworkStream);
            MST_plugin = ReplaceImageFromBase64((string) obj["image_MST_plugin"], MST_pluginStream);
            OSO_logo = ReplaceImageFromBase64((string) obj["image_OSO_logo"], OSO_logoStream);

            List<string> categoryStrings = ((string) obj["categories"]).Split(',').ToList();
            foreach (var categoryString in categoryStrings)
            {
                Categories.CategoryNames.Add(new Category {Name = (string)categoryString});
            }
        

        }
        public void Load(IVstPlugin plugin)
        {
            if (plugin == null || !plugin.IsScanned)
            {
                return;
            }

            var files = GetFiles(plugin);


            if (File.Exists(files["color"]))
            {
                Color = JsonConvert.DeserializeObject<Color>(
                    File.ReadAllText(files["color"]));

                Color.BackgroundColor =
                    (System.Windows.Media.Color) ColorConverter.ConvertFromString("#" + Color.VB_bgcolor);
            }
            else
            {
                Color.BackgroundColor = (System.Windows.Media.Color) ColorConverter.ConvertFromString("#000000");
            }

            if (File.Exists(files["shortname"]))
            {
                ShortNames = JsonConvert.DeserializeObject<ShortNames>(
                    File.ReadAllText(files["shortname"]));
            }
            else
            {
                ShortNames.VB_shortname = plugin.PluginName;
                ShortNames.MKII_shortname = plugin.PluginName;
                ShortNames.MIKRO_shortname = plugin.PluginName;
                ShortNames.MST_shortname = plugin.PluginName;
            }

            if (File.Exists(files["categories"]))
            {
                Categories = JsonConvert.DeserializeObject<Categories>(
                    File.ReadAllText(files["categories"]));

                Categories.CategoryNames.Clear();
                if (Categories.CategoryDB.Count == 1)
                {
                    var categoryStrings = Categories.CategoryDB.First().Categories.ToArray();
                    foreach (var categoryString in categoryStrings)
                    {
                        Categories.CategoryNames.Add(new Category {Name = categoryString});
                    }
                }
                else
                {
                    var categoryDb = new CategoryDB();
                    if (plugin.PluginType == VstHost.PluginTypes.Instrument)
                    {
                        categoryDb.FileType = "INST";
                    }
                    else
                    {
                        categoryDb.FileType = "FX";
                    }

                    Categories.CategoryDB.Add(categoryDb);
                    Categories.Vendor = plugin.PluginVendor;
                    Categories.Product = plugin.PluginName;
                }
            }
            else
            {
                var categoryDb = new CategoryDB();
                if (plugin.PluginType == VstHost.PluginTypes.Instrument)
                {
                    categoryDb.FileType = "INST";
                }
                else
                {
                    categoryDb.FileType = "FX";
                }

                Categories.CategoryDB.Add(categoryDb);
                Categories.Vendor = plugin.PluginVendor;
                Categories.Product = plugin.PluginName;
            }

            if (File.Exists(files["VB_logo"]))
            {
                VB_logo = ReplaceImage(files["VB_logo"], VB_logoStream);
            }

            if (File.Exists(files["VB_artwork"]))
            {
                VB_artwork = ReplaceImage(files["VB_artwork"], VB_artworkStream);
            }

            if (File.Exists(files["MST_logo"]))
            {
                MST_logo = ReplaceImage(files["MST_logo"], MST_logoStream);
            }

            if (File.Exists(files["MST_artwork"]))
            {
                MST_artwork = ReplaceImage(files["MST_artwork"], MST_artworkStream);
            }

            if (File.Exists(files["MST_plugin"]))
            {
                MST_plugin = ReplaceImage(files["MST_plugin"], MST_pluginStream);
            }

            if (File.Exists(files["OSO_logo"]))
            {
                OSO_logo = ReplaceImage(files["OSO_logo"], OSO_logoStream);
            }
        }
        
        

        public BitmapImage ReplaceImage(string fileName, MemoryStream targetStream)
        {
            targetStream.SetLength(0);

            var bytes = File.ReadAllBytes(fileName);
            targetStream.Write(bytes, 0, bytes.Length);
            targetStream.Seek(0, SeekOrigin.Begin);

            var targetImage = new BitmapImage();
            targetImage.BeginInit();
            targetImage.StreamSource = targetStream;
            targetImage.EndInit();

            return targetImage;
        }
        
        public BitmapImage ReplaceImageFromBase64(string base64, MemoryStream targetStream)
        {
            targetStream.SetLength(0);

            var bytes = Convert.FromBase64String(base64);
            targetStream.Write(bytes, 0, bytes.Length);
            targetStream.Seek(0, SeekOrigin.Begin);

            var targetImage = new BitmapImage();
            targetImage.BeginInit();
            targetImage.StreamSource = targetStream;
            targetImage.EndInit();

            return targetImage;
        }
    }
}