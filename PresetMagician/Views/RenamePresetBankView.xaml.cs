using System.Windows;
using Catel.Windows;
using PresetMagician.ViewModels;

namespace PresetMagician.Views
{
    public partial class RenamePresetBankView
    {
        public RenamePresetBankView(RenamePresetBankViewModel viewModel)
            : base(viewModel, DataWindowMode.OkCancel)
        {
            InitializeComponent();
        }
    }
}
