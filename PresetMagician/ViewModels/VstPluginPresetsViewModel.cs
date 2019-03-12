using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Catel.Data;
using Catel.MVVM;
using GongSolutions.Wpf.DragDrop;
using MethodTimer;
using PresetMagician.Core.Data;
using PresetMagician.Services.Interfaces;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;

namespace PresetMagician.ViewModels
{
    public sealed class VstPluginPresetsViewModel : ViewModelBase, IDropTarget, IModelTracker
    {
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        
        public VstPluginPresetsViewModel(Plugin plugin, IRuntimeConfigurationService runtimeConfigurationService)
        {
            Plugin = plugin;

            PresetsView = (ListCollectionView) CollectionViewSource.GetDefaultView(Plugin.Presets);
            PresetsView.IsLiveSorting = false;
            PresetsView.IsLiveFiltering = false;

            PropertyChanged += OnPropertyChanged;
            
            Title = $"{plugin.PluginName}: Presets";
            _runtimeConfigurationService = runtimeConfigurationService;
            _runtimeConfigurationService.ApplicationState.IsApplicationEditing = true;
            
            RenameBankCommand = new TaskCommand(OnRenameBankCommandExecute);
            //ThrottlingRate = new TimeSpan(0, 0, 0, 0, 500);
        }
        
        /// <summary>
        /// Gets the ShowKeyboardMappings command.
        /// </summary>
        public TaskCommand RenameBankCommand { get; private set; }

        /// <summary>
        /// Method to invoke when the ShowKeyboardMappings command is executed.
        /// </summary>
        private async Task OnRenameBankCommandExecute()
        {
            Debug.WriteLine("FOO");
        }

        protected override Task OnClosedAsync(bool? result)
        {
            _runtimeConfigurationService.ApplicationState.IsApplicationEditing = false;
            return base.OnClosedAsync(result);
        }

        private bool PresetFilter(Preset preset)
        {
            if (SelectedTreeNode != null)
            {
                return SelectedTreeNode.IsEqualOrBelow(preset.PresetBank);
            }

            return true;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {            
            if (e.PropertyName == nameof(SelectedTreeNode))
            {
                PresetsView.Filter = o => PresetFilter(o as Preset);
            }
        }

        public IUserEditable GetTrackedModel()
        {
            return Plugin;
        }

        #region Properties

        public PresetBank SelectedTreeNode { get; set; }

        public ListCollectionView PresetsView { get; private set; }

        [Model]
        public Plugin Plugin { get; protected set; }

        #endregion

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
                var sourceBank = dropInfo.Data as PresetBank;
                if (dropInfo.TargetItem.GetType() == typeof(PresetBank))
                {
                    var targetBank = dropInfo.TargetItem as PresetBank;
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
                    var targetBank = dropInfo.TargetItem as PresetBank;
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
                var sourceBank = dropInfo.Data as PresetBank;
                var sourceParentBank = sourceBank.ParentBank;

                if (dropInfo.TargetItem.GetType() == typeof(PresetBank))
                {
                    var targetBank = dropInfo.TargetItem as PresetBank;

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
                    presets = new List<Preset>() { dropInfo.Data as Preset };
                }
                else
                {
                    presets = dropInfo.Data as List<Preset>;
                }
                
                if (dropInfo.TargetItem.GetType() == typeof(PresetBank))
                {
                    var targetBank = dropInfo.TargetItem as PresetBank;
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