using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Catel.Collections;
using Catel.Data;
using Catel.MVVM;
using Catel.Services;
using GongSolutions.Wpf.DragDrop;
using PresetMagician.Core.Data;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;
using Type = PresetMagician.Core.Models.Type;

namespace PresetMagician.ViewModels
{
    public sealed class VstPluginPresetsViewModel : ViewModelBase, IDropTarget, IModelTracker
    {
        private readonly IUIVisualizerService _visualizerService;
        private readonly DataPersisterService _dataPersisterService;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly IAdvancedMessageService _messageService;

        public VstPluginPresetsViewModel(Plugin plugin, GlobalService globalService,
            IUIVisualizerService visualizerService,
            DataPersisterService dataPersisterService, IViewModelFactory viewModelFactory,
            IAdvancedMessageService advancedMessageService)
        {
            _messageService = advancedMessageService;
            _visualizerService = visualizerService;
            _viewModelFactory = viewModelFactory;
            _dataPersisterService = dataPersisterService;

            Plugin = plugin;

            PresetsView = (ListCollectionView) CollectionViewSource.GetDefaultView(Plugin.Presets);
            PresetsView.IsLiveSorting = false;
            PresetsView.IsLiveFiltering = false;

            Title = $"{plugin.PluginName}: Presets";

            GlobalCharacteristicCollection =
                (ListCollectionView) CollectionViewSource.GetDefaultView(globalService.GlobalCharacteristics);
            GlobalCharacteristicCollection.SortDescriptions.Add(
                new SortDescription(nameof(Characteristic.EffectiveCharacteristicName), ListSortDirection.Ascending));
            GlobalCharacteristicCollection.IsLiveSorting = false;
            GlobalCharacteristicCollection.IsLiveFiltering = false;

            GlobalTypeCollection = (ListCollectionView) CollectionViewSource.GetDefaultView(globalService.GlobalTypes);
            GlobalTypeCollection.SortDescriptions.Add(new SortDescription(nameof(Type.EffectiveFullTypeName),
                ListSortDirection.Ascending));
            GlobalTypeCollection.IsLiveSorting = false;
            GlobalTypeCollection.IsLiveFiltering = false;

            PreviewNotePlayers = globalService.PreviewNotePlayers;
            RenameBankCommand = new TaskCommand(OnRenameBankCommandExecute, RenameBankCommandCanExecute);
            AddBankCommand = new TaskCommand(OnAddBankCommandExecute, AddBankCommandCanExecute);
            DeleteBankCommand = new TaskCommand(OnDeleteBankCommandExecute, DeleteBankCommandCanExecute);

            Revert = new Command<string>(OnRevertExecute);
        }
        
        #region Properties

        public PresetBank SelectedTreeNode { get; set; }
        public ListCollectionView PresetsView { get; }
        [Model] public Plugin Plugin { get; }
        public ListCollectionView GlobalTypeCollection { get; }
        public FastObservableCollection<PreviewNotePlayer> PreviewNotePlayers { get; }
        public ListCollectionView GlobalCharacteristicCollection { get; }
        public Preset SelectedPreset { get; set; }
        public List<Preset> SelectedPresets { get; set; }
        public bool HasSelectedPreset { get; set; }
        #endregion


        protected override Task<bool> CancelAsync()
        {
            Plugin.CancelEdit();
            return base.CancelAsync();
        }

        protected override Task<bool> SaveAsync()
        {
            Plugin.EndEdit();

            try
            {
                _dataPersisterService.SavePresetsForPlugin(Plugin);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return base.SaveAsync();
        }

        protected override Task InitializeAsync()
        {
            Plugin.BeginEdit();
            return base.InitializeAsync();
        }

       

        #region Command Rename Bank

        public TaskCommand RenameBankCommand { get; }

        private async Task OnRenameBankCommandExecute()
        {
            await _visualizerService.ShowDialogAsync<PresetBankViewModel>((
                plugin: Plugin, presetBank: SelectedTreeNode, parentBank: SelectedTreeNode.ParentBank));
        }

        private bool RenameBankCommandCanExecute()
        {
            return SelectedTreeNode != null && !SelectedTreeNode.IsVirtualBank;
        }

        #endregion

        #region Command Delete Bank

        public TaskCommand DeleteBankCommand { get; }

        private async Task OnDeleteBankCommandExecute()
        {
            bool isEmpty = true;
            foreach (var preset in Plugin.Presets)
            {
                if (SelectedTreeNode.IsEqualOrBelow(preset.PresetBank))
                {
                    isEmpty = false;
                    break;
                }
            }

            if (!isEmpty)
            {
                await _messageService.ShowErrorAsync(
                    "Cannot delete a preset bank which contains presets. Please move the affected presets to another bank prior deleting.",
                    "Cannot delete preset bank");
            }
            else
            {
                SelectedTreeNode.ParentBank.PresetBanks.Remove(SelectedTreeNode);
            }
            
        }

        private bool DeleteBankCommandCanExecute()
        {
            return SelectedTreeNode != null && !SelectedTreeNode.IsVirtualBank;
        }

        #endregion

        #region Command Add Bank

        public TaskCommand AddBankCommand { get; }

        private async Task OnAddBankCommandExecute()
        {
            var newBank = new PresetBank {BankName = ""};
            var viewModel = _viewModelFactory.CreateViewModel<PresetBankViewModel>((
                plugin: Plugin, presetBank: newBank, parentBank: SelectedTreeNode));

            viewModel.Title = "Add Preset Bank";
            viewModel.SavedAsync += ViewModelOnSavedAsync;
            await _visualizerService.ShowDialogAsync(viewModel);
        }

        private Task ViewModelOnSavedAsync(object sender, EventArgs e)
        {
            var presetBank = ((PresetBankViewModel) sender).PresetBank;
            SelectedTreeNode.PresetBanks.Add(presetBank);
            return Task.CompletedTask;
        }

        private bool AddBankCommandCanExecute()
        {
            return SelectedTreeNode != null;
        }

        #endregion

        #region Command Revert

        public Command<string> Revert { get; }


        private void OnRevertExecute(string parameter)
        {
            switch (parameter)
            {
                case nameof(PresetMetadata.PresetName):
                    SelectedPreset.Metadata.PresetName = SelectedPreset.OriginalMetadata.PresetName;
                    break;
                case nameof(PresetMetadata.Author):
                    SelectedPreset.Metadata.Author = SelectedPreset.OriginalMetadata.Author;
                    break;
                case nameof(PresetMetadata.Comment):
                    SelectedPreset.Metadata.Comment = SelectedPreset.OriginalMetadata.Comment;
                    break;
                case nameof(PresetMetadata.BankPath):
                    SelectedPreset.Metadata.BankPath = SelectedPreset.OriginalMetadata.BankPath;
                    break;
                case nameof(PresetMetadata.Characteristics):
                    SelectedPreset.Metadata.Characteristics.SynchronizeCollection(
                        SelectedPreset.OriginalMetadata.Characteristics);
                    break;
                case nameof(PresetMetadata.Types):
                    SelectedPreset.Metadata.Types.SynchronizeCollection(
                        SelectedPreset.OriginalMetadata.Types);
                    break;
            }
        }

        #endregion

        public Predicate<object> ExternalFilter { get; set; }

        private bool PresetFilter(Preset preset)
        {
            if (SelectedTreeNode != null)
            {
                return SelectedTreeNode.IsEqualOrBelow(preset.PresetBank);
            }

            return true;
        }

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedTreeNode))
            {
                ExternalFilter =  o => PresetFilter(o as Preset);
                
                RenameBankCommand.RaiseCanExecuteChanged();
                AddBankCommand.RaiseCanExecuteChanged();
                DeleteBankCommand.RaiseCanExecuteChanged();
            }

            if (e.PropertyName == nameof(SelectedPreset))
            {
                HasSelectedPreset = SelectedPreset != null;
            }
        }

        public IUserEditable GetTrackedModel()
        {
            return Plugin;
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data == null)
            {
                return;
            }

            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;

            if (dropInfo.TargetItem == null)
            {
                return;
            }

            if (dropInfo.Data.GetType() == typeof(PresetBank))
            {
                var sourceBank = (PresetBank)dropInfo.Data;
                if (dropInfo.TargetItem.GetType() == typeof(PresetBank))
                {
                    var targetBank = (PresetBank) dropInfo.TargetItem;
                    var sourceParentBank = sourceBank.ParentBank;
                    if (!targetBank.IsVirtualBank && sourceParentBank != targetBank)
                    {
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                        dropInfo.Effects = DragDropEffects.Move;
                    }
                }
            }

            if (dropInfo.Data.GetType() == typeof(Preset) || dropInfo.Data.GetType() == typeof(List<Preset>))
            {
                if (dropInfo.TargetItem.GetType() == typeof(PresetBank))
                {
                    var targetBank = (PresetBank) dropInfo.TargetItem;
                    if (!targetBank.IsVirtualBank)
                    {
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                        dropInfo.Effects = DragDropEffects.Move;
                    }
                }
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data == null || dropInfo.TargetItem == null)
            {
                return;
            }

            if (dropInfo.Data.GetType() == typeof(PresetBank))
            {
                var sourceBank = (PresetBank)dropInfo.Data;
                var sourceParentBank = sourceBank.ParentBank;

                if (dropInfo.TargetItem.GetType() == typeof(PresetBank))
                {
                    var targetBank = (PresetBank)dropInfo.TargetItem;

                    if (!targetBank.IsVirtualBank && sourceBank != targetBank)
                    {
                        sourceParentBank.PresetBanks.Remove(sourceBank);
                        sourceParentBank.Refresh();
                        targetBank.PresetBanks.Add(sourceBank);
                        targetBank.Refresh();
                    }
                }
            }

            if (dropInfo.Data.GetType() == typeof(Preset) || dropInfo.Data.GetType() == typeof(List<Preset>))
            {
                List<Preset> presets;

                if (dropInfo.Data.GetType() == typeof(Preset))
                {
                    presets = new List<Preset>() {(Preset)dropInfo.Data};
                }
                else
                {
                    presets = (List<Preset>)dropInfo.Data;
                }

                if (dropInfo.TargetItem.GetType() == typeof(PresetBank))
                {
                    var targetBank = (PresetBank)dropInfo.TargetItem;
                    if (!targetBank.IsVirtualBank)
                    {
                        foreach (var preset in presets)
                        {
                            preset.PresetBank = targetBank;
                        }
                    }
                }
            }
        }
    }
}