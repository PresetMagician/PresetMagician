using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using PresetMagician.VendorPresetParser.Common;
using PresetMagician.Core.Models;

namespace PresetMagician.VendorPresetParser.SlateDigital
{
    public abstract class SlateDigitalPresetParser : RecursiveBankDirectoryParser
    {
        protected abstract string PresetSectionName { get; }
        protected override string Extension { get; } = "epf";

        private void RetrievePresetData(XNode node, PresetParserMetadata preset)
        {
            preset.Comment = GetNodeValue(node,
                $"string(/package/archives/archive[@client_id='{PresetSectionName}-preset']/section/entry[@id='Preset notes']/@value)");
            preset.Author = GetNodeValue(node,
                $"string(/package/archives/archive[@client_id='{PresetSectionName}-preset']/section/entry[@id='Preset author']/@value)");
        }

        public override async Task DoScan()
        {
            await PluginInstance.LoadPlugin();
            await base.DoScan();
        }

        protected override byte[] ProcessFile(string fileName, PresetParserMetadata preset)
        {
            var xmlPreset = XDocument.Load(fileName);
            var chunk = PluginInstance.GetChunk(false);
            var chunkXml = Encoding.UTF8.GetString(chunk);

            var actualPresetDocument = XDocument.Parse(chunkXml);

            RetrievePresetData(xmlPreset, preset);

            MigrateData(xmlPreset, actualPresetDocument, preset);

            var builder = new StringBuilder();
            using (TextWriter writer = new StringWriter(builder))
            {
                actualPresetDocument.Save(writer);
                return Encoding.UTF8.GetBytes(builder.ToString());
            }
        }

        private void MigrateData(XNode source, XNode dest, PresetParserMetadata preset)
        {
            var dataNode =
                $"/package/archives/archive[@client_id='{PresetSectionName}-preset']/section[@id='ParameterValues']/entry";

            var nodes = source.XPathSelectElements(dataNode);

            var dataNode2 =
                $"/package/archives/archive[@client_id='{PresetSectionName}-state']/section[@id='Slot0']";
            var insertNode = dest.XPathSelectElement(dataNode2);
            insertNode.Elements().Remove();
            insertNode.Add(nodes);

            var presetNameNode = new XElement("entry");
            presetNameNode.SetAttributeValue("id", "Preset name");
            presetNameNode.SetAttributeValue("type", "string");

            var bank = RootBank.CreateRecursive(preset.BankPath);
            presetNameNode.SetAttributeValue("value", bank.BankName + "/" + preset.PresetName);

            insertNode.Add(presetNameNode);
        }

        private string GetNodeValue(XNode document, string xpath)
        {
            return document.XPathEvaluate(xpath) as string;
        }
    }
}