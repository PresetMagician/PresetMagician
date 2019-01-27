using System;
using Catel.IoC;
using Orchestra;
using Orchestra.Services;
using PresetMagician;
using PresetMagician.Services;

/// <summary>
/// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
/// </summary>
// ReSharper disable once CheckNamespace
public static class ModuleInitializer
{
    /// <summary>
    /// Initializes the module.
    /// </summary>
    public static void Initialize()
    {
        Program.startTime = DateTime.Now;

        var serviceLocator = ServiceLocator.Default;

        serviceLocator.RegisterType<ISplashScreenService, PresetMagician.Services.SplashScreenService>();
        serviceLocator.RegisterType<IRibbonService, RibbonService>();
        serviceLocator.RegisterType<IApplicationInitializationService, ApplicationInitializationService>();

        //InitializeThirdPartyNotices();

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
            "PresetMagician",
            "Resources.ThirdPartyNotices.GSF.txt")
        );

        thirdPartyNoticesService.AddWithTryCatch(() => new ResourceBasedThirdPartyNotice(
            "MahApps.Metro.IconPacks",
            "https://github.com/MahApps/MahApps.Metro.IconPacks",
            "PresetMagician",
            "Resources.ThirdPartyNotices.MahApps.Metro.IconPacks.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "MessagePack-CSharp",
            "https://github.com/neuecc/MessagePack-CSharp",
            "PresetMagician",
            "Resources.ThirdPartyNotices.MessagePack.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "NAudio",
            "https://github.com/naudio/NAudio",
            "PresetMagician",
            "Resources.ThirdPartyNotices.NAudio.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "JSON.NET",
            "https://github.com/JamesNK/Newtonsoft.Json",
            "PresetMagician",
            "Resources.ThirdPartyNotices.Newtonsoft.Json.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "Fody, PropertyChanged.Fody, MethodTimer.Fody, ModuleInit.Fody, LoadAssembliesOnStartup.Fody",
            "https://github.com/Fody",
            "PresetMagician",
            "Resources.ThirdPartyNotices.Fody.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "Syroot.Windows.IO.KnownFolders",
            "https://gitlab.com/Syroot/KnownFolders",
            "PresetMagician",
            "Resources.ThirdPartyNotices.Syroot.Windows.IO.KnownFolders.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "WPFHexaEditor",
            "https://github.com/abbaye/WPFHexEditorControl",
            "PresetMagician",
            "Resources.ThirdPartyNotices.WPFHexaEditor.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "Catel.Fody",
            "https://github.com/Catel/Catel.Fody",
            "PresetMagician",
            "Resources.ThirdPartyNotices.Catel.Fody.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "Extended.Wpf.Toolkit",
            "https://github.com/xceedsoftware/wpftoolkit",
            "PresetMagician",
            "Resources.ThirdPartyNotices.Extended.Wpf.Toolkit.txt")
        );

        thirdPartyNoticesService.Add(new ResourceBasedThirdPartyNotice(
            "Dirkster.AvalonDock.Themes.VS2013",
            "https://github.com/Dirkster99/AvalonDock",
            "PresetMagician",
            "Resources.ThirdPartyNotices.Dirkster.AvalonDock.Themes.VS2013.txt")
        );
    }
}