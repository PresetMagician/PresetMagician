using System;
using Catel;
using Catel.IoC;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;
using PresetMagicianShell.ViewModels;

namespace PresetMagicianShell.Services
{
    public class VstService: IVstService
    {
        private readonly IServiceLocator _serviceLocator;

        private Plugin _selectedPlugin;
        public Plugin SelectedPlugin
        {
            get { return _selectedPlugin; }
            set
            {
                _selectedPlugin = value;
                SelectedPluginChanged.SafeInvoke(this);
            }
        }
        
        public event EventHandler SelectedPluginChanged;

        public VstService (IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public void RefreshPluginList ()
        {
            var vstPluginViewModel = _serviceLocator.ResolveType<VstPluginsViewModel>();
            vstPluginViewModel.RefreshPluginList.Execute();
        }
    }
}