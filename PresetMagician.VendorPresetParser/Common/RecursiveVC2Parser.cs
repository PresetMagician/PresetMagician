using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using GSF;
using PresetMagician.Core.Models;

namespace PresetMagician.VendorPresetParser.Common
{
    public abstract class RecursiveVC2Parser : RecursiveBankDirectoryParser
    {
        protected static Regex XmlHeaderReplacerRegex = new Regex(@"<\?xml.*?\?>", RegexOptions.Compiled);
        protected Func<string, string> PreProcessXmlFunc;

        public void SetPreProcessXmlFunction(Func<string, string> func)
        {
            PreProcessXmlFunc = func;
        }

        protected override byte[] ProcessFile(string fileName, PresetParserMetadata preset)
        {
            var data = File.ReadAllText(fileName);

            if (PreProcessXmlFunc != null)
            {
                data = PreProcessXmlFunc.Invoke(data);
            }

            var xmlWithoutHeader = XmlHeaderReplacerRegex.Replace(data, "");

            return WriteVC2(xmlWithoutHeader).ToArray();
        }

        public static MemoryStream WriteVC2(string pluginData)
        {
            var ms = new MemoryStream();

            ms.Write(new byte[] {0x56, 0x43, 0x32, 0x21}, 0, 4);
            var data = Encoding.UTF8.GetBytes(pluginData);
            ms.Write(LittleEndian.GetBytes(data.Length), 0, 4);
            ms.Write(data, 0, data.Length);
            ms.WriteByte(0);

            return ms;
        }
    }
}