using Catel.IoC;
using PresetMagicianShell.Services.Interfaces;
using PresetMagicianShell.ViewModels;

namespace PresetMagicianShell.Services
{
    public class VstService: IVstService
    {
        private readonly IServiceLocator _serviceLocator;

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