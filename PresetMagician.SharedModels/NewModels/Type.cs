using System.Collections.Generic;
using Ceras;
using SharedModels.Data;

namespace SharedModels.NewModels
{
    public class Type: ModelBase
    {
        public override ICollection<string> EditableProperties { get; } = new List<string>
        {
            nameof(TypeName),
            nameof(SubTypeName)
        };
        
        public static readonly GlobalTypeCollection GlobalTypes = new GlobalTypeCollection();

        [Include] public string TypeName { get; set; } = "";
        [Include] public string SubTypeName { get; set; } = "";
    }
}