// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplashScreenViewModel.cs" company="WildGums">
//   Copyright (c) 2008 - 2016 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Orchestra.Services;

namespace PresetMagicianShell.ViewModels
{
    using Catel;
    using Catel.MVVM;
    using Catel.Services;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// The splash screen view model.
    /// </summary>
    public class SplashScreenViewModel : ViewModelBase
    {
        private readonly IAboutInfoService _aboutInfoService;
        private readonly ILanguageService _languageService;

        public SplashScreenViewModel(IAboutInfoService aboutInfoService, ILanguageService languageService)
        {
            Argument.IsNotNull(() => aboutInfoService);
            Argument.IsNotNull(() => languageService);

            _aboutInfoService = aboutInfoService;
            _languageService = languageService;
        }

        #region Properties

        public static bool IsActive { get; private set; }

        public Uri CompanyLogoForSplashScreenUri { get; private set; }

        public string Company { get; private set; }

        public string ProducedBy { get; private set; }

        public string Version { get; private set; }

        public string Action { get; set; }

        #endregion Properties

        #region Methods

        protected override async Task InitializeAsync()
        {
            IsActive = true;

            await base.InitializeAsync();

            var aboutInfo = _aboutInfoService.GetAboutInfo();

            Title = aboutInfo.Name;
            Company = aboutInfo.Company;

            CompanyLogoForSplashScreenUri = aboutInfo.CompanyLogoForSplashScreenUri;
            ProducedBy = string.Format(_languageService.GetString("Orchestra_ProducedBy"), "foobar" + Company);
            Version = aboutInfo.DisplayVersion;
            Action = "Initializing...";
        }

        protected override Task OnClosedAsync(bool? result)
        {
            IsActive = false;

            return base.OnClosedAsync(result);
        }

        #endregion Methods
    }
}