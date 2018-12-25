using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Drachenkatze.PresetMagician.VendorPresetParser.Common;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    public abstract class AudioThing: AbstractVendorPresetParser
    {
        protected const string BankNameFactory = "Factory";

        protected string ConfigNode;

        protected void DoScan(PresetBank rootBank, string directory)
        {
            

            var dirInfo = new DirectoryInfo(directory);
            foreach (var file in dirInfo.EnumerateFiles("*.atp"))
            {
                Preset preset = new Preset();
                preset.PresetName = file.Name.Replace(".atp", "");
                preset.SetPlugin(VstPlugin);
                preset.PresetBank = rootBank;

                var data = File.ReadAllText(file.FullName);

                var xmlPreset = XDocument.Parse(data);
                var presetElement = xmlPreset.Element(ConfigNode);

                ParserCallback(presetElement);
                var ms = VC2Writer.WriteVC2(presetElement.ToString());
                preset.PresetData = ms.ToArray();
                Presets.Add(preset);
            }

            foreach (var subDirectory in dirInfo.EnumerateDirectories())
            {
                var bank = new PresetBank
                {
                    BankName = subDirectory.Name
                };

                DoScan(bank, subDirectory.FullName);
                rootBank.PresetBanks.Add(bank);
            }
        }


        public void ParserCallback(XElement element)
        {
           
        }
    }
}
