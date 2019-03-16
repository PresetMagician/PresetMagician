using System;
using System.Collections.Generic;
using Ceras;
using Ceras.Resolvers;
using PresetMagician.Core.Data;

namespace PresetMagician.Core.Models
{
    public interface IType
    {
        string TypeName { get; set; }
        string SubTypeName { get; set; }
    }

    public class Type : ModelBase, IType
    {
        private static HashSet<string> _editableProperties = new HashSet<string>
        {
            nameof(TypeName),
            nameof(SubTypeName),
            nameof(IsRedirect),
            nameof(RedirectType)
        };

        public override HashSet<string> GetEditableProperties()
        {
            return _editableProperties;
        }

        public static readonly GlobalTypeCollection GlobalTypes = new GlobalTypeCollection();

        private string _typeName = "";
        private string _subTypeName = "";

        [Include]
        public string TypeName
        {
            get { return _typeName; }
            set
            {
                if (value == null)
                {
                    _typeName = "";
                }
                else
                {
                    _typeName = value;
                }
            }
        }

        [Include]
        public string SubTypeName
        {
            get { return _subTypeName; }
            set
            {
                if (value == null)
                {
                    _subTypeName = "";
                }
                else
                {
                    _subTypeName = value;
                }
            }
        }

        private bool _isRedirect;

        [Include]
        public bool IsRedirect
        {
            get => _isRedirect;
            set
            {
                _isRedirect = value;
                if (!_isRedirect)
                {
                    RedirectType = null;
                }
            }
        }

        [Include] public Type RedirectType { get; set; }

        public bool HasSeparator
        {
            get
            {
                if (SubTypeName == "")
                {
                    return false;
                }

                return true;
            }
        }

        public string FullTypeName
        {
            get
            {
                if (SubTypeName != "")
                {
                    return TypeName + ">" + SubTypeName;
                }

                return TypeName;
            }
        }
        
        public string EffectiveFullTypeName
        {
            get
            {
                if (EffectiveSubTypeName != "")
                {
                    return EffectiveTypeName + ">" + EffectiveSubTypeName;
                }

                return EffectiveTypeName;
            }
        }

        public string EffectiveTypeName
        {
            get
            {
                if (IsRedirect && RedirectType != null)
                {
                    return RedirectType.TypeName;
                }

                return TypeName;
            }
        }

        public string EffectiveSubTypeName
        {
            get
            {
                if (IsRedirect && RedirectType != null)
                {
                    return RedirectType.SubTypeName;
                }

                return SubTypeName;
            }
        }

        public string RedirectDescription => IsRedirect ? $"Redirected from {TypeName} {SubTypeName}" : "";
    }
}