using System;
using PresetMagicianShell.Models;

namespace PresetMagicianShell.Services.Interfaces
{
    public interface IVstService
    {
        #region Methods
        void RefreshPluginList();
        #endregion
        event EventHandler SelectedPluginChanged;
        Plugin SelectedPlugin { get; set; }
    }
}