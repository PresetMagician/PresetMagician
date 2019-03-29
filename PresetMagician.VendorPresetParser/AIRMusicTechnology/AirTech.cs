using System;
using PresetMagician.VendorPresetParser.Common;
using Microsoft.Win32;
using PresetMagician.Core.Models;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology
{
    public abstract class AirTech: RecursiveBankDirectoryParser
    {
        protected string GetContentRegistryValue(string key, string name)
        {
            using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(key))
            {
                if (regKey == null)
                {
                    throw new Exception($"Unable to open registry key {key}");
                }

                var value = regKey.GetValue(name);

                var valueKind = regKey.GetValueKind("Content");
                if (valueKind != RegistryValueKind.String)
                {
                    throw new Exception($"Expected registry key to be of type REG_SZ (String), found {valueKind}");
                }

                if (value == null)
                {
                    throw new Exception($"Registry value for {key} {name} is null");
                }

                return value.ToString();
            }
        }

        private (string directory, string filename) GetSplittedParseFile(string fileName)
        {
            var parseDirectories = GetParseDirectories();
            var patchDirectory = "";
            
            foreach (var parseDirectory in parseDirectories)
            {
                if (fileName.Contains(parseDirectory.directory))
                {
                    patchDirectory = parseDirectory.directory;
                }   
            }
            
            string file;
            
            if (patchDirectory.EndsWith("\\"))
            {
                file = fileName.Replace(patchDirectory, "");
                
            }
            else
            {
                file = fileName.Replace(patchDirectory + "\\", "");
            }

            return (patchDirectory, file);
        }

        protected abstract Tfx.Tfx GetTfxParser();
        
        protected override byte[] ProcessFile(string fileName, PresetParserMetadata preset)
        {
            var tfx = GetTfxParser();

                var split = GetSplittedParseFile(fileName);
           
            tfx.Parse(split.directory, split.filename);


            return tfx.GetDataToWrite();
        }
    }
}