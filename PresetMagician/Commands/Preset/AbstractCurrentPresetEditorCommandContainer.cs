using System.ComponentModel;
using Catel.MVVM;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.ViewModels;

namespace PresetMagician
{
    public abstract class AbstractCurrentPresetEditorCommandContainer : CommandContainerBase
    {
        protected VstPluginPresetsViewModel CurrentPresetViewModel;
        private readonly GlobalFrontendService _globalFrontendService;

        public AbstractCurrentPresetEditorCommandContainer(string command, ICommandManager commandManager,
            GlobalFrontendService globalFrontendService)
            : base(command, commandManager)
        {
            _globalFrontendService = globalFrontendService;

            _globalFrontendService.ApplicationState.PropertyChanged += ApplicationStateOnPropertyChanged;
        }

        private void ApplicationStateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ApplicationState.CurrentDocumentViewModel))
            {
                var oldModel = CurrentPresetViewModel;
                CurrentPresetViewModel =
                    _globalFrontendService.ApplicationState.CurrentDocumentViewModel as VstPluginPresetsViewModel;
                OnCurrentPresetViewModelChanged(oldModel, CurrentPresetViewModel);
                InvalidateCommand();
            }
        }

        protected virtual void OnCurrentPresetViewModelChanged(VstPluginPresetsViewModel oldModel,
            VstPluginPresetsViewModel newModel)
        {
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && CurrentPresetViewModel != null;
        }
    }
}