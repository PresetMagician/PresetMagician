using System;
using Catel.Windows;
using PresetMagician.ViewModels;

namespace PresetMagician.Views
{
    public partial class RegistrationWindow
    {
        public RegistrationWindow() : this(null)
        {
        }

        public RegistrationWindow(RegistrationViewModel viewModel)
            : base(viewModel, DataWindowMode.Custom)
        {
            InitializeComponent();

        }


        protected override void OnClosed(EventArgs e)
        {
            var x = (RegistrationViewModel) ViewModel;

            if (!x.ValidLicense)
            {
                x.CloseApplication.Execute();
            }

            base.OnClosed(e);
        }
    }
}