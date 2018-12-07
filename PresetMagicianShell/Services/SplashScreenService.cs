// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplashScreenService.cs" company="WildGums">
//   Copyright (c) 2008 - 2014 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using Catel;
using Orchestra.Services;
using PresetMagicianShell.Services.EventArgs;
using PresetMagicianShell.ViewModels;

namespace PresetMagicianShell.Services
{
    using System.Windows;

    public class SplashScreenService : ISplashScreenService
    {
        private Views.SplashScreen _splashScreen;

        public Window CreateSplashScreen()
        {
            _splashScreen = new Views.SplashScreen(this);
            return _splashScreen;
        }

        public event EventHandler<StartupActionChangedEventArgs> ActionChanged;

        private string _action;

        public String Action
        {
            get => _action;
            set
            {
                _action = value;
                ActionChanged.SafeInvoke(this, new StartupActionChangedEventArgs(value));
            }
        }
    }
}