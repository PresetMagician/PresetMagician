using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Services;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsPatchPluginToAudioOutputCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly RemoteVstService _remoteVstService;
        private readonly GlobalService _globalService;

        public PluginToolsPatchPluginToAudioOutputCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator)
            : base(Commands.PluginTools.PatchPluginToAudioOutput, commandManager, serviceLocator)
        {
            _remoteVstService = ServiceLocator.ResolveType<RemoteVstService>();
            _globalService = ServiceLocator.ResolveType<GlobalService>();
            _globalFrontendService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _globalFrontendService.SelectedPlugins.Count == 1;
        }

        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            var pluginInstance =
                _remoteVstService.GetInteractivePluginInstance(_globalFrontendService.SelectedPlugin);

            if (!pluginInstance.IsLoaded)
            {
                await pluginInstance.LoadPlugin();
            }


            pluginInstance.PatchPluginToAudioOutput(_globalService.RuntimeConfiguration.AudioOutputDevice);

            foreach (var dev in _globalService.RuntimeConfiguration.MidiInputDevices)
            {
                try
                {
                    pluginInstance.PatchPluginToMidiInput(dev);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }
    }
}