using System;
using Orchestra.Models;
using Orchestra.Services;

namespace PresetMagicianShell.Services
{

    internal class AboutInfoService : IAboutInfoService
    {
        public AboutInfo GetAboutInfo()
        {
            var aboutInfo = new AboutInfo(
                uriInfo: new UriInfo("https://presetmagician.drachenkatze.org", "PresetMagician Homepage"),
                name: "PresetMagician"
            ) {ShowLogButton = false};


            return aboutInfo;
        }
    }
}