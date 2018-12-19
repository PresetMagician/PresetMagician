using System;
using Catel;
using Catel.Collections;
using Catel.IoC;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;
using PresetMagicianShell.ViewModels;

namespace PresetMagicianShell.Services
{
    public class VstService : IVstService
    {
        private readonly IServiceLocator _serviceLocator;
        public VstHost VstHost { get; set; }
        public VstService(IServiceLocator serviceLocator)
        {
            Argument.IsNotNull(() => serviceLocator);

            _serviceLocator = serviceLocator;
            VstHost = new VstHost();

          
        }


        public FastObservableCollection<Plugin> SelectedPlugins { get; } = new FastObservableCollection<Plugin>();
        public FastObservableCollection<Plugin> Plugins { get; private set; } = new FastObservableCollection<Plugin>();

       
        #region SelectedPlugin

        private Plugin _selectedPlugin;

        public Plugin SelectedPlugin
        {
            get => _selectedPlugin;
            set
            {
                _selectedPlugin = value;
                SelectedPluginChanged.SafeInvoke(this);
            }
        }

        public event EventHandler SelectedPluginChanged;

        #endregion
    }
}