using System.ComponentModel;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.Services;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetRenamePresetBankCommandContainer : AbstractCurrentPresetEditorCommandContainer
    {
        private readonly IUIVisualizerService _uiVisualizerService;

        public PresetRenamePresetBankCommandContainer(ICommandManager commandManager,
            IUIVisualizerService uiVisualizerService, GlobalFrontendService globalFrontendService)
            : base(Commands.Preset.RenamePresetBank, commandManager, globalFrontendService)
        {
            Argument.IsNotNull(() => uiVisualizerService);
            _uiVisualizerService = uiVisualizerService;
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && CurrentPresetViewModel?.SelectedTreeNode != null &&
                   !CurrentPresetViewModel.SelectedTreeNode.IsVirtualBank;
        }

        protected override void OnCurrentPresetViewModelChanged(VstPluginPresetsViewModel oldModel,
            VstPluginPresetsViewModel newModel)
        {
            if (oldModel != null)
            {
                oldModel.PropertyChanged -= ModelOnPropertyChanged;
            }

            if (newModel != null)
            {
                newModel.PropertyChanged += ModelOnPropertyChanged;
            }

            base.OnCurrentPresetViewModelChanged(oldModel, newModel);
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VstPluginPresetsViewModel.SelectedTreeNode))
            {
                InvalidateCommand();
            }
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            await _uiVisualizerService.ShowDialogAsync<RenamePresetBankViewModel>((
                plugin: CurrentPresetViewModel.Plugin, presetBank: CurrentPresetViewModel.SelectedTreeNode));
        }
    }
}