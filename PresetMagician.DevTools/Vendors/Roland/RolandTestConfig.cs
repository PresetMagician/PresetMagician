using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PresetMagician.VendorPresetParser.Roland.Internal;

namespace PresetMagician.DevTools.Vendors.Roland
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RolandTestConfig
    {
        [JsonProperty] public string OutputDirectory;
        [JsonProperty] public string Plugin;

        [JsonProperty] public string ScriptFile;
        [JsonProperty] public RolandExportConfig ExportConfig;
        [JsonProperty] public List<string> PresetDirectories = new List<string>();
        [JsonProperty] public List<string> ComparisonIgnoreValuePaths = new List<string>();
        [JsonProperty] public List<string> ComparisonIgnoreValueRegex = new List<string>();
        [JsonProperty] public Dictionary<string, int> ComparisonAllowValueVariance= new Dictionary<string, int>();
        
        

        public static RolandTestConfig LoadTestConfig(string testConfigPath)
        {
            var testConfig = JsonConvert.DeserializeObject<RolandTestConfig>(File.ReadAllText(testConfigPath));

            var exportConfig = Path.Combine(Path.GetDirectoryName(testConfigPath), "ExportConfig.json");
            var suffixData = Path.Combine(Path.GetDirectoryName(testConfigPath), "suffix.data");
            
            testConfig.ExportConfig = JsonConvert.DeserializeObject<RolandExportConfig>(File.ReadAllText(exportConfig));
            testConfig.ExportConfig.Suffix = File.ReadAllBytes(suffixData);
            
            
            return testConfig;
        }
        
        public Dictionary<string, Regex> RegexDictionary = new Dictionary<string, Regex>();

        public Regex GetRegex(string regex)
        {
            if (!RegexDictionary.ContainsKey(regex))
            {
                var r = new Regex(regex, RegexOptions.Compiled);
                RegexDictionary.Add(regex, r);
            }


            return RegexDictionary[regex];
        }

        public bool ShouldIgnoreValuePath(string valuePath)
        {
            if (ComparisonIgnoreValuePaths.Contains(valuePath))
            {
                return true;
            }

            foreach (var r in ComparisonIgnoreValueRegex)
            {
                var re = GetRegex(r);
                
                if (re.Match(valuePath).Success)
                {
                    return true;
                }
            }

            return false;
        }
        
        
    }
}