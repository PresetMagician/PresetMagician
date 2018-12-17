using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel;
using Catel.Data;
using Catel.Fody;
using Catel.MVVM;
using PresetMagicianShell.Services.Interfaces;

namespace PresetMagicianShell.ViewModels
{
    public class VstPluginViewModel : ViewModelBase
    {
        private readonly IVstService _vstService;

        public VstPluginViewModel(IVstService vstService)
        {
            Argument.IsNotNull(() => vstService);
            _vstService = vstService;
            _vstService.SelectedPluginChanged += OnSelectedPluginChanged;
            Plugin = _vstService.SelectedPlugin;
            DoSomething = new Command(OnDoSomethingExecute);
            
        }

        private void OnSelectedPluginChanged(object o, EventArgs e)
        {
            Plugin = _vstService.SelectedPlugin;
            RaisePropertyChanged(nameof(Plugin));
        }

        public Command DoSomething  { get; set; }

        private void OnDoSomethingExecute ()
        {
            Debug.WriteLine("Direct: "+Plugin.Presets.Count);
        }

        public bool IsPluginSet
        {
            get { return Plugin != null; }
        }

        public Models.Plugin Plugin { get; protected set; }
      
    }

}