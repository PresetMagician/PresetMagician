using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PresetMagicianScratchPad.Roland
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RolandExportConfig
    {
        [JsonProperty]
        public bool ExportZeroForInt1X7 { get; set; } = true;

        [JsonProperty] public List<string> SkipExportValuePaths = new List<string>();

        [JsonProperty] public List<string> SkipExportValuePathsRegEx = new List<string>();
        
        [JsonProperty] public List<string> IncludeExportValuePaths = new List<string>();

        [JsonProperty] public List<string> IncludeExportValuePathsRegEx = new List<string>();
        
        [JsonProperty] public List<string> PresetDirectories = new List<string>();
        
        [JsonProperty] public List<string> SkipImportValuePaths = new List<string>();
        
        [JsonProperty] public List<string> SkipImportValuePathsRegEx = new List<string>();

        [JsonProperty] public string Plugin;

        [JsonProperty] public string ScriptFile;
        
        [JsonProperty] public string CallbackHook;
        
        [JsonProperty] public string SuffixFile;
        
        [JsonProperty] public string OutputDirectory;
        
        [JsonProperty] public string KoaFileHeader;
        [JsonProperty] public int PresetNameLength;
        [JsonProperty] public int PresetLength;
        
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


    }
}