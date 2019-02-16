using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Catel.Collections;
using Catel.Data;
using Drachenkatze.PresetMagician.Utils;
using SharedModels;
using System.Linq;
using Catel.IoC;
using Catel.Runtime.Serialization;
using MethodTimer;
using PresetMagician.Serialization;
using Catel.Runtime.Serialization.Xml;
using XmlSerializer = System.Xml.Serialization.XmlSerializer;

namespace PresetMagicianScratchPad
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ServiceLocator.Default.RegisterType<IXmlSerializer, TestSerializer>();
            ServiceLocator.Default.RegisterType<ISerializer, TestSerializer>();
            
            var plugin = new Plugin();
            var preset = new Preset {Bar = "I'm unmodified, too!"};
            plugin.Presets.Add(preset);

            var ms = new MemoryStream();
            XmlSerializer xs = new XmlSerializer(typeof(Plugin));
            xs.Serialize(ms, plugin);
            
            
            (plugin as IEditableObject).BeginEdit();
            preset.Foo = "I'm now modified";
            preset.Bar = "I'm now modified as well";
            (plugin as IEditableObject).CancelEdit();

            Debug.WriteLine(preset.Foo);
            Debug.WriteLine(preset.Bar);

     
        }

    }
    
    public class Preset: ModelBase
    {
        public string Foo { get; set; } = "I'm unmodified";
        public string Bar { get; set; }
    
    }

    public class Plugin : ChildAwareModelBase 
    {
        [IncludeInSerialization]
        public ObservableCollection<Preset> Presets { get; set; } = new ObservableCollection<Preset>();
        public void ClearDirty () {
            IsDirty = false;
        }
    }
}