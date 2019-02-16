using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Catel.IoC;
using Catel.IO;
using Catel.Runtime.Serialization;
using Catel.Runtime.Serialization.Xml;

namespace PresetMagician.Serialization
{
    public class TestSerializer : XmlSerializer
    {
        public TestSerializer(ISerializationManager serializationManager, IDataContractSerializerFactory dataContractSerializerFactory, IXmlNamespaceManager xmlNamespaceManager, ITypeFactory typeFactory, IObjectAdapter objectAdapter) : base(serializationManager, dataContractSerializerFactory, xmlNamespaceManager, typeFactory, objectAdapter)
        {
        }

        public override List<MemberValue> GetSerializableMembers(ISerializationContext<XmlSerializationContextInfo> context, object model, params string[] membersToIgnore)
        {
            var x =  base.GetSerializableMembers(context, model, membersToIgnore);
            var sb = new StringBuilder();
            foreach (var i in x)
            {
                sb.Append(i.Name + " ");
            }
            
            Debug.WriteLine(sb);

            return x;

        }

        protected override SerializationObject DeserializeMember(ISerializationContext<XmlSerializationContextInfo> context, MemberValue memberValue)
        {
            if (memberValue.Name == "BankPath")
            {
                Debug.WriteLine(memberValue.Name);
            }

            return base.DeserializeMember(context, memberValue);
        }

        public override void SerializeMembers(object model, Stream stream, ISerializationConfiguration configuration,
            params string[] membersToIgnore)
        {
            base.SerializeMembers(model, stream, configuration, membersToIgnore);
            Debug.WriteLine(stream.GetUtf8String());
        }

        public override void Serialize(object model, Stream stream, ISerializationConfiguration configuration = null)
        {
            base.Serialize(model, stream, configuration);
            Debug.WriteLine(stream.GetUtf8String());
        }
    }
}