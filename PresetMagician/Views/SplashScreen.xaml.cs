// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplashScreen.xaml.cs" company="WildGums">
//   Copyright (c) 2008 - 2014 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows.Threading;
using Orc.Controls;
using Orchestra.Services;
using PresetMagician.Services.EventArgs;
using PresetMagician.ViewModels;
using SplashScreenService = PresetMagician.Services.SplashScreenService;

namespace PresetMagician.Views
{
    using System.Windows;
    using System.Windows.Media;
    using Catel.Windows;

    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SplashScreen" /> class.
        /// </summary>
        public SplashScreen(ISplashScreenService splashScreenService)
            : base(DataWindowMode.Custom)
        {
            InitializeComponent();

            var x = (SplashScreenService)splashScreenService;
            x.ActionChanged += OnFoobar;
        }

        private static readonly Action EmptyDelegate = delegate { };

        private void OnFoobar(Object sender, StartupActionChangedEventArgs e)
        {
            var x = (SplashScreenViewModel)this.ViewModel;
            x.Action = e.NewValue;
            this.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        #endregion Constructors
    }
}