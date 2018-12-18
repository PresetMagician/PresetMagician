using MahApps.Metro.IconPacks;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using Catel.Windows;
using PresetMagicianShell.ViewModels;

namespace PresetMagicianShell.Views
{
    public partial class SettingsView
    {
        public SettingsView(SettingsViewModel viewModel)
            : base(viewModel, DataWindowMode.OkCancel)
        {
            InitializeComponent();
        }
       
    }
}
