using System;
using Catel.Collections;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Services
{
    public class GlobalFrontendService
    {
        public GlobalFrontendService()
        {
            ApplicationState = new ApplicationState();
        }

        public FastObservableCollection<Plugin> SelectedPlugins { get; } = new FastObservableCollection<Plugin>();
        public FastObservableCollection<Preset> SelectedPresets { get; } = new FastObservableCollection<Preset>();
        public FastObservableCollection<Preset> PresetExportList { get; } = new FastObservableCollection<Preset>();

        #region SelectedPlugin

        private Plugin _selectedPlugin;

        public Plugin SelectedPlugin
        {
            get => _selectedPlugin;
            set
            {
                _selectedPlugin = value;
                SelectedPluginChanged?.Invoke(this, System.EventArgs.Empty);
            }
        }

        public event EventHandler SelectedPluginChanged;

        #endregion

        #region SelectedExportPreset

        private Preset _selectedExportPreset;

        public Preset SelectedExportPreset
        {
            get => _selectedExportPreset;
            set
            {
                _selectedExportPreset = value;
                SelectedExportPresetChanged?.Invoke(this, System.EventArgs.Empty);
            }
        }

        public event EventHandler SelectedExportPresetChanged;

        #endregion

        public RuntimeConfiguration EditableConfiguration { get; set; }
        public ApplicationState ApplicationState { get; }
    }
}