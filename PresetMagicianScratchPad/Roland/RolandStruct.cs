using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace PresetMagicianScratchPad.Roland
{
    public class RolandStruct : RolandMemorySection
    {
        public RolandStruct(RolandMemorySection parent, XElement node)
        {
            Parent = parent;
            SourceNode = node;
        }

        public override void Parse()
        {
            base.Parse();
            ApplyType();
        }

       

        public override void ApplyProperties()
        {
            foreach (var childElement in SourceNode.Elements())
            {
                var elementValue = childElement.Value;

                switch (childElement.Name.ToString())
                {
                    case "name":
                        Name = elementValue;
                        break;
                    case "address":
                        if (!HasOffset)
                        {
                            Offset = ParseAddress(elementValue);
                            StartAddress = Parent.StartAddress + Offset;
                        }

                        break;
                    case "type":
                        break;
                    default:
                        Debug.WriteLine(
                            $"ApplyProperties: Unknown element {childElement.Name} at {((IXmlLineInfo) childElement).LineNumber}");
                        break;
                }
            }
        }


        /// <summary>
        /// Applies the type to this memory section
        /// </summary>
        /// <exception cref="Exception"></exception>
        public virtual void ApplyType()
        {
            var typeElement = SourceNode.Element("type");

            if (typeElement == null)
            {
                throw new Exception(
                    $"Found a struct with no type definition in line {((IXmlLineInfo) typeElement).LineNumber}");
            }

            Type = typeElement.Value;

            var structTypeNode = GetStructType(SourceNode.Document, typeElement.Value);

            var childStartOffset = 0;
            foreach (var childElement in structTypeNode.Elements())
            {
                var elementValue = childElement.Value;
                switch (childElement.Name.ToString())
                {
                    case "name":
                        if (elementValue != "$name")
                        {
                            if (!string.IsNullOrWhiteSpace(Name))
                            {
                                Debug.WriteLine(
                                    $"ApplyType: Overwriting name '{Name}' with '{elementValue}' from structType {typeElement.Value} at {((IXmlLineInfo) SourceNode).LineNumber}");
                            }

                            Name = elementValue;
                        }


                        break;
                    case "address":
                        if (elementValue != "$address")
                        {
                            throw new Exception(
                                $"ApplyType: Unexpected absolute address from structType {typeElement.Value} at {((IXmlLineInfo) SourceNode).LineNumber}");
                        }

                        break;
                    case "size":
                        IsAutoCalculatedSize = false;
                        Size = ParseSize(elementValue);
                        break;
                    case "struct":
                        break;
                    case "value":
                    case "value-rvs":
                        break;
                    case "type":
                        break;
                    default:
                        Debug.WriteLine(
                            $"ParseStruct: Unknown type {childElement.Name} at {((IXmlLineInfo) childElement).LineNumber}");
                        break;
                }
            }

            DoCallback("RolandStructBeforeApplyChilds");
            int fileStartAddress = FileAddress;
            foreach (var childElement in structTypeNode.Elements())
            {
                switch (childElement.Name.ToString())
                {
                    case "struct":
                        var addressElements = childElement.Elements("address").ToList();

                        if (addressElements.Count == 0)
                        {
                            var subStruct = new RolandStruct(this, childElement);
                            subStruct.Callbacks = Callbacks;
                            subStruct.StartAddress = StartAddress + childStartOffset;
                            subStruct.FileAddress = fileStartAddress;

                            subStruct.Parse();

                            if (!subStruct.HasOffset)
                            {
                                childStartOffset += subStruct.Size;
                            }
                            else
                            {
                                subStruct.StartAddress = StartAddress + subStruct.Offset;
                                var newOffset = subStruct.Offset + subStruct.Size;
                                if (newOffset > childStartOffset)
                                {
                                    childStartOffset = newOffset;
                                }
                            }


                            Structs.Add(subStruct);
                            fileStartAddress += subStruct.FileSize;
                        }
                        else
                        {
                            foreach (var addressElement in addressElements)
                            {
                                var subStruct = new RolandStruct(this, childElement);
                                subStruct.Callbacks = Callbacks;
                                subStruct.FileAddress = fileStartAddress;
                                subStruct.Offset = ParseAddress(addressElement.Value);
                                subStruct.StartAddress = StartAddress + subStruct.Offset;

                                subStruct.HasOffset = true;
                                subStruct.Parse();

                                if (!subStruct.HasOffset)
                                {
                                    childStartOffset += subStruct.Size;
                                }
                                else
                                {
                                    subStruct.StartAddress = StartAddress + subStruct.Offset;
                                    var newOffset = subStruct.Offset + subStruct.Size;
                                    if (newOffset > childStartOffset)
                                    {
                                        childStartOffset = newOffset;
                                    }
                                }


                                Structs.Add(subStruct);
                                fileStartAddress += subStruct.FileSize;
                            }
                        }

                        break;
                    case "foovalue":
                    case "foovalue-rvs":
                        RolandValue value;

                        if (childElement.Elements("address").Count() > 1)
                        {
                            var useArray = true;
                            foreach (var addr in childElement.Elements("address"))
                            {
                                if (string.IsNullOrEmpty(addr.Value))
                                {
                                    useArray = false;
                                }
                            }

                            if (useArray)
                            {
                                value = new RolandValueArray(this, childElement);
                            }
                            else
                            {
                                value = new RolandValue(this, childElement);
                            }
                        }
                        else if (childElement.Elements("size").Any())
                        {
                            value = new RolandValueArray(this, childElement);
                        } else
                        {
                            value = new RolandValue(this, childElement);
                        }

                        value.StartAddress = StartAddress + childStartOffset;
                        value.FileAddress = fileStartAddress;
                        value.Callbacks = Callbacks;
                        value.Parse();

                        if (!value.HasOffset)
                        {
                            childStartOffset += value.Size;
                            fileStartAddress += value.FileSize;
                            
                        }
                        else
                        {
                            
                            value.StartAddress = StartAddress + value.Offset;
                            value.FileAddress = FileAddress + value.Offset;
                            
                            var newOffset = value.Offset + value.Size;
                            var fileOffset = value.Offset + value.FileSize;
                            if (newOffset > childStartOffset)
                            {
                                childStartOffset = newOffset;
                                
                            }

                            if (fileOffset > fileStartAddress - FileAddress)
                            {
                                fileStartAddress = FileAddress + newOffset;
                            }
                            
                            
                        }

                        Values.Add(value);
                        

                        break;
                }
            }

            FileSize = fileStartAddress - FileAddress;
            
            if (IsAutoCalculatedSize)
            {
                Size = childStartOffset;
            }
        }
    }
}