using System.Diagnostics;
using System.Net;
using Catel.IoC;
using Orchestra;
using Orchestra.Services;
using PresetMagicianShell.Services;

/// <summary>
/// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
/// </summary>
public static class ModuleInitializer
{
    /// <summary>
    /// Initializes the module.
    /// </summary>
    public static void Initialize()
    {
        var serviceLocator = ServiceLocator.Default;

        serviceLocator.RegisterType<ISplashScreenService, PresetMagicianShell.Services.SplashScreenService>();
        serviceLocator.RegisterType<IRibbonService, RibbonService>();
        serviceLocator.RegisterType<IApplicationInitializationService, ApplicationInitializationService>();

        InitializeThirdPartyNotices();
        // ***** IMPORTANT NOTE *****
        //
        // Only register the shell services in the ModuleInitializer. All other types must be registered
        // in the ApplicationInitializationService
    }

    private static void InitializeThirdPartyNotices()
    {
        var serviceLocator = ServiceLocator.Default;

        var thirdPartyNoticesService = serviceLocator.ResolveType<IThirdPartyNoticesService>();
        thirdPartyNoticesService.AddWithTryCatch(() => new ResourceBasedThirdPartyNotice(
            "GSF Core, GSF Media",
            "https://github.com/GridProtectionAlliance/gsf",
            "PresetMagicianShell",
            "Resources.ThirdPartyNotices.GSF.txt")
        );

        thirdPartyNoticesService.AddWithTryCatch(() => new ResourceBasedThirdPartyNotice(
            "MahApps.Metro.IconPacks",
            "https://github.com/MahApps/MahApps.Metro.IconPacks",
            "PresetMagicianShell",
            "Resources.ThirdPartyNotices.MahApps.Metro.IconPacks.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "MessagePack-CSharp",
            "https://github.com/neuecc/MessagePack-CSharp",
            "PresetMagicianShell",
            "Resources.ThirdPartyNotices.MessagePack.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "NAudio",
            "https://github.com/naudio/NAudio",
            "PresetMagicianShell",
            "Resources.ThirdPartyNotices.NAudio.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "JSON.NET",
            "https://github.com/JamesNK/Newtonsoft.Json",
            "PresetMagicianShell",
            "Resources.ThirdPartyNotices.Newtonsoft.Json.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "Orc.LicenseManager",
            "https://github.com/WildGums/Orc.LicenseManager",
            "PresetMagicianShell",
            "Resources.ThirdPartyNotices.Orc.LicenseManager.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "Fody, PropertyChanged.Fody, MethodTimer.Fody, ModuleInit.Fody, LoadAssembliesOnStartup.Fody",
            "https://github.com/Fody",
            "PresetMagicianShell",
            "Resources.ThirdPartyNotices.Fody.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "Syroot.Windows.IO.KnownFolders",
            "https://gitlab.com/Syroot/KnownFolders",
            "PresetMagicianShell",
            "Resources.ThirdPartyNotices.Syroot.Windows.IO.KnownFolders.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "WPFHexaEditor",
            "https://github.com/abbaye/WPFHexEditorControl",
            "PresetMagicianShell",
            "Resources.ThirdPartyNotices.WPFHexaEditor.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "Catel.Fody",
            "https://github.com/Catel/Catel.Fody",
            "PresetMagicianShell",
            "Resources.ThirdPartyNotices.Catel.Fody.txt")
        );
    }
}