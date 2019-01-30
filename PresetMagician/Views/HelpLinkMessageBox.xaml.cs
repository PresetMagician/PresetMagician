// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBox.xaml.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using Orchestra;

namespace PresetMagician.Views
{
    using Catel.Services;
    using Catel.Windows;
    using ViewModels;

    public partial class HelpLinkMessageBox
    {
        #region Constructors
        public HelpLinkMessageBox()
            : this(null)
        {
        }

        public HelpLinkMessageBox(HelpLinkMessageBoxViewModel viewModel)
            : base(viewModel, DataWindowMode.Custom)
        {
            InitializeComponent();

            if (viewModel.Button == MessageButton.YesNo)
            {
                this.DisableCloseButton();
            }
        }
        #endregion
    }
}