using System;
using System.ComponentModel;
using System.Diagnostics;
using Catel;
using Catel.Services;
using Catel.Windows;
using PresetMagicianShell.ViewModels;

namespace PresetMagicianShell.Views
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

            viewModel.LicenseUpdated += OnLicenseUpdated;

        }

        private void OnLicenseUpdated(object o, EventArgs e)
        {
            Close();
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
