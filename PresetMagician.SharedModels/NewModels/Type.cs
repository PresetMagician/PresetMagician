using System.Collections.Generic;
using System.Xml.Serialization;
using Ceras;
using SharedModels.Data;
using SharedModels.Models;

namespace SharedModels.NewModels
{
    public class Type: ModelBase
    {
        public override ICollection<string> EditableProperties { get; } = new List<string>
        {
            nameof(TypeName),
            nameof(SubTypeName)
        };
        
        public static readonly Dictionary<(string TypeName, string SubTypeName), Type> GlobalTypes = new Dictionary<(string, string), Type>();

        public string TypeName { get; set; } = "";
        public string SubTypeName { get; set; } = "";
    }
}