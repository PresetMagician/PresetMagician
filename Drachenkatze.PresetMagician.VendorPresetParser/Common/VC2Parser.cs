using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Anotar.Catel;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Common
{
    public class VC2Parser
    {
        protected string Extension;
        protected IVstPlugin _vstPlugin;
        protected ObservableCollection<Preset> Presets { get; }

        protected Regex XmlHeaderReplacerRegex;
        
        public VC2Parser(IVstPlugin vstPlugin, string extension, ObservableCollection<Preset> presets)
        {
            _vstPlugin = vstPlugin;
            Extension = extension;
            Presets = presets;
            XmlHeaderReplacerRegex = new Regex(@"<\?xml.*?\?>", RegexOptions.Compiled);
        }
        
        public void DoScan(PresetBank rootBank, string directory)
        {
            var dirInfo = new DirectoryInfo(directory);
            foreach (var file in dirInfo.EnumerateFiles("*."+Extension))
            {
                try
                {
                    Preset preset = new Preset();
                    preset.PresetName = file.Name.Replace("." + Extension, "");
                    preset.SetPlugin(_vstPlugin);
                    preset.PresetBank = rootBank;

                    var data = File.ReadAllText(file.FullName);

                    var xmlWithoutHeader = XmlHeaderReplacerRegex.Replace(data, "");

                    var ms = VC2Writer.WriteVC2(xmlWithoutHeader);
                   
                    preset.PresetData = ms.ToArray();
                    
                    Presets.Add(preset);
                } catch (Exception e)
                {
                    LogTo.Error("Error processing preset {0} because of {1} {2}", file.FullName, e.Message, e.ToString());
                    LogTo.Debug(e.StackTrace);
                }
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
        
        
    }
}