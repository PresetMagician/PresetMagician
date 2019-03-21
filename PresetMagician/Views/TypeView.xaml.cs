using Catel.Windows;
using PresetMagician.ViewModels;

namespace PresetMagician.Views
{
    public partial class TypeView
    {
        public TypeView(TypeViewModel viewModel) : base(viewModel, DataWindowMode.OkCancel)
        {
            InitializeComponent();
        }
    }
}