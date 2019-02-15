using System.Collections.Specialized;
using System.ComponentModel;
using Catel;
using Catel.MVVM;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;

namespace PresetMagician
{
    public abstract class AbstractCurrentPresetEditorCommandContainer: CommandContainerBase
    {
        protected IRuntimeConfigurationService RuntimeConfigurationService;
        protected VstPluginPresetsViewModel CurrentPresetViewModel;
        
        public AbstractCurrentPresetEditorCommandContainer(string command, ICommandManager commandManager, IRuntimeConfigurationService runtimeConfigurationService)
            : base(command, commandManager)
        {
            Argument.IsNotNull(() => runtimeConfigurationService);

            RuntimeConfigurationService = runtimeConfigurationService;
            
            runtimeConfigurationService.ApplicationState.PropertyChanged += ApplicationStateOnPropertyChanged;
        }

        private void ApplicationStateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ApplicationState.CurrentDocumentViewModel))
            {
                var oldModel = CurrentPresetViewModel;
                CurrentPresetViewModel =
                    RuntimeConfigurationService.ApplicationState.CurrentDocumentViewModel as VstPluginPresetsViewModel;
                OnCurrentPresetViewModelChanged(oldModel, CurrentPresetViewModel);
                InvalidateCommand();
            }
        }

        protected virtual void OnCurrentPresetViewModelChanged(VstPluginPresetsViewModel oldModel, VstPluginPresetsViewModel newModel)
        {
            
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && CurrentPresetViewModel != null;
        }
    }
}