using System.Collections.Generic;
using Ceras;
using PresetMagician.Core.Data;

namespace PresetMagician.Core.Models
{
    public interface IType
    {
        string TypeName { get; set; }
        string SubTypeName { get; set; }
    }

    public class Type: ModelBase, IType
    {
        private static HashSet<string> _editableProperties = new HashSet<string>  {
            nameof(TypeName),
            nameof(SubTypeName)
        };
            
        public override HashSet<string> GetEditableProperties()
        {
            return _editableProperties;
        }
        
        public static readonly GlobalTypeCollection GlobalTypes = new GlobalTypeCollection();

        [Include] public string TypeName { get; set; } = "";
        [Include] public string SubTypeName { get; set; } = "";
    }
}