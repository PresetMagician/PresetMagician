using System;
using System.Collections.Generic;
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
            ExportNode = new XElement("struct");
        }

        private Dictionary<string, int> ArrayStructCounters = new Dictionary<string, int>();

        public override void Parse()
        {
            base.Parse();
            ApplyType();
            Debug.WriteLine($"Parsed {ValuePath}");
        }


        public override void ApplyProperties()
        {
            foreach (var childElement in SourceNode.Elements().ToList())
            {
                var elementValue = childElement.Value;

                switch (childElement.Name.ToString())
                {
                    case "name":
                        Name = elementValue;
                        ParentValuePath.AddRange(Parent.ParentValuePath);


                        ParentValuePath.Add(Name);


                        break;
                    case "address":
                        if (!HasOffset)
                        {
                            //StartAddress = ParseAddress(elementValue, Parent.ChildOffset);
                            //StartAddress = Parent.StartAddress + Offset;
                        }

                        break;
                    case "type":
                        break;
                    default:
                        if (!childElement.Name.ToString().StartsWith("int_"))
                        {
                            Debug.WriteLine(
                                $"ApplyProperties: Unknown element {childElement.Name} at {((IXmlLineInfo) childElement).LineNumber}");
                        }

                        break;
                }
            }

            ValuePath = string.Join(".", ParentValuePath);
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
            foreach (var childElement in structTypeNode.Elements().ToList())
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
                            else
                            {
                                if (UseArrayIndex)
                                {
                                    Name = elementValue + "[" + ArrayIndex + "]";
                                }
                                else
                                {
                                    Name = elementValue;
                                }


                                ParentValuePath.AddRange(Parent.ParentValuePath);


                                ParentValuePath.Add(Name);
                                ValuePath = string.Join(".", ParentValuePath);
                            }
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
                        FileSize = Size;
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


            ExportNode.Add(new XElement("int_valuePath") {Value = ValuePath});
            ExportNode.Add(new XElement("int_calculatedName") {Value = Name});

            DoCallback("RolandStructBeforeApplyChilds");
            int fileStartAddress = FileAddress;
            foreach (var childElement in structTypeNode.Elements().ToList())
            {
                switch (childElement.Name.ToString())
                {
                    case "struct":

                        ApplyFoo(childElement, false);
                       

                        break;
                    case "value":
                    case "value-rvs":
                        ApplyFoo(childElement, true);
                        break;
                }
            }

            //

            if (IsAutoCalculatedSize)
            {
                FileSize = FileOffset;
                Size = ChildOffset;
            }
            else
            {
                FileSize = FileOffset;
            }

            CalculatedSize = ChildOffset;
        }

        public void ApplyFoo(XElement childElement, bool isValue)
        {
            if (ExportNode.Element("int_children") == null)
            {
                ExportNode.Add(new XElement("int_children"));
            }

            var addressElements = childElement.Elements("address").ToList();

            if (addressElements.Count == 0)
            {
                var child = CreateChild(childElement, isValue);
                child.StartAddress = StartAddress + ChildOffset;
                child.FileAddress = FileAddress + FileOffset;

                child.Parse();

                ChildOffset += child.Size;
                FileOffset += child.FileSize;
                if (isValue)
                {
                    Values.Add((RolandValue) child);
                }
                else
                {
                    Structs.Add((RolandStruct) child);
                }

                var cs = new XElement(child.ExportNode);
                child.ApplyDebugProperties(cs);
                ExportNode.Element("int_children").Add(cs);
            }
            else
            {
                var arrayIndex = 0;

                bool useArrayIndex = addressElements.Count > 1;

                foreach (var addressElement in addressElements)
                {
                    var childOffset = ParseAddress(addressElement.Value, 0);
                    var fileOffset = isValue ? ParseAddress(addressElement.Value, 0) : FileOffset;


                    var childName = childElement.Element("type").Value;
                    if (!isValue)
                    {
                        Debug.WriteLine(
                            $"ParseAddress for {childName} {addressElement.Value} with file offset {FileOffset:X} results in filestart {FileAddress:X} wih offset {fileOffset:X}. ChildOffset is {childOffset:X}");
                    }

                    var child = CreateChild(childElement, isValue);
                    child.StartAddress = StartAddress + childOffset;
                    child.FileAddress = FileAddress + fileOffset;
                    child.UseArrayIndex = useArrayIndex;
                    child.ArrayIndex = arrayIndex;
                    child.Parse();


                    arrayIndex++;

                    if (childOffset + child.Size > ChildOffset)
                    {
                        ChildOffset = childOffset + child.Size;
                    }

                    if (fileOffset + child.FileSize > FileOffset)
                    {
                        FileOffset = fileOffset + child.FileSize;
                    }

                    if (isValue)
                    {
                        Values.Add((RolandValue) child);
                    }
                    else
                    {
                        Structs.Add((RolandStruct) child);
                    }

                    var cs = new XElement(child.ExportNode);
                    child.ApplyDebugProperties(cs);
                    ExportNode.Element("int_children").Add(cs);
                }
            }
        }

        public RolandMemorySection CreateChild(XElement childElement, bool isValue)
        {
            RolandMemorySection child;

            if (isValue)
            {
                child = new RolandValue(this, childElement);
            }
            else
            {
                child = new RolandStruct(this, childElement);
            }

            return child;
        }
    }
}