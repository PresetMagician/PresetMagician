using Orchestra.Models;
using Orchestra.Services;

namespace PresetMagician.Services
{
    internal class AboutInfoService : IAboutInfoService
    {
        public AboutInfo GetAboutInfo()
        {
            var aboutInfo = new AboutInfo(
                uriInfo: new UriInfo(Settings.Links.Homepage, "PresetMagician Homepage"),
                name: "PresetMagician"
            ) {ShowLogButton = false};


            return aboutInfo;
        }
    }
}