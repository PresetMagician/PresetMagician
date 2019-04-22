using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PresetMagician.VendorPresetParser.Roland.Internal
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RolandExportConfig
    {
        [JsonProperty] public List<string> SkipExportValuePaths = new List<string>();

        [JsonProperty] public List<string> SkipExportValuePathsRegEx = new List<string>();

        [JsonProperty] public List<string> IncludeExportValuePaths = new List<string>();

        [JsonProperty] public List<string> IncludeExportValuePathsRegEx = new List<string>();


        [JsonProperty] public List<string> SkipImportValuePaths = new List<string>();

        [JsonProperty] public List<string> SkipImportValuePathsRegEx = new List<string>();
        
        [JsonProperty] public List<string> PostProcessors = new List<string>();


        public byte[] Suffix;


        [JsonProperty] public string KoaFileHeader;
        [JsonProperty] public int KoaPresetNameLength;
        [JsonProperty] public int KoaPresetLength;
        [JsonProperty] public int KoaNumPresets;
        
        private Dictionary<string, bool> ShouldExportValuePath = new Dictionary<string, bool>();
      
        

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

        public bool ShouldExport(string valuePath)
        {
            if (!ShouldExportValuePath.ContainsKey(valuePath))
            {
                var shouldExport = !(ShouldExclude(valuePath) && !ShouldForceInclude(valuePath));
                
                ShouldExportValuePath.Add(valuePath, shouldExport);
            }

            return ShouldExportValuePath[valuePath];
        }

        private bool ShouldForceInclude(string valuePath)
        {
            bool forceInclude = IncludeExportValuePaths.Contains(valuePath);

            foreach (var reg in IncludeExportValuePathsRegEx)
            {
                var r = GetRegex(reg);
                if (r.Match(valuePath).Success)
                {
                    forceInclude = true;
                    break;
                }
            }

            return forceInclude;
        }

        private bool ShouldExclude(string valuePath)
        {
            if (SkipExportValuePaths.Contains(valuePath))
            {
                return true;
            }

            foreach (var reg in SkipExportValuePathsRegEx)
            {
                var r = GetRegex(reg);
                if (r.Match(valuePath).Success)
                {
                    return true;
                }
            }

            return false;
        }
    }
}