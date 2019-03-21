// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplashScreenService.cs" company="WildGums">
//   Copyright (c) 2008 - 2014 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Windows;
using Orchestra.Services;
using PresetMagician.Services.EventArgs;
using SplashScreen = PresetMagician.Views.SplashScreen;

namespace PresetMagician.Services
{
    public class SplashScreenService : ISplashScreenService
    {
        private SplashScreen _splashScreen;

        public Window CreateSplashScreen()
        {
            _splashScreen = new SplashScreen(this);
            return _splashScreen;
        }

        public event EventHandler<StartupActionChangedEventArgs> ActionChanged;

        private string _action;

        public string Action
        {
            get => _action;
            set
            {
                _action = value;
                ActionChanged?.Invoke(this, new StartupActionChangedEventArgs(value));
            }
        }
    }
}