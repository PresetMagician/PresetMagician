using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Catel.Collections;
using Catel.Data;
using Drachenkatze.PresetMagician.Utils;

namespace PresetMagicianScratchPad
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var plugin = new Plugin();
            var preset = new Preset();
            plugin.Presets.Add(preset);
            preset.Foo = "bar";
            preset.ClearDirty();
            plugin.ClearDirty();
            
           (plugin as IEditableObject).BeginEdit();

           Debug.WriteLine($"State of plugin.IsDirty: {plugin.IsDirty}");
           Debug.WriteLine($"State of preset.IsDirty: {preset.IsDirty}");

           preset.Foo = "Foo";
           
           Debug.WriteLine($"State of plugin.IsDirty after editing: {plugin.IsDirty}");
           Debug.WriteLine($"State of preset.IsDirty after editing: {preset.IsDirty}");
        }

    }
    
    public class Preset: ModelBase 
    {
        public string Foo {get;set;} public void ClearDirty () {
            IsDirty = false;
        }

        protected override void OnBeginEdit(BeginEditEventArgs e)
        {
            Debug.WriteLine("Beginning edit on preset");
            base.OnBeginEdit(e);
        }
    }

    public class Plugin : ChildAwareModelBase 
    {
        public string foo { get; set; }
        public FastObservableCollection<Preset> Presets { get; set; } = new FastObservableCollection<Preset>();
        
        protected override void OnBeginEdit(BeginEditEventArgs e)
        {
            Debug.WriteLine("Beginning edit on plugin");
            base.OnBeginEdit(e);
        }
        
        public void ClearDirty () {
            IsDirty = false;
        }
    }
}