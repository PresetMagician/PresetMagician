using System.Threading.Tasks;
using PresetMagicianShell.Models;

namespace PresetMagicianShell.Services.Interfaces
{
    public interface IRuntimeConfigurationService
    {
        #region Methods
        void LoadConfiguration();
        void SaveConfiguration();
        #endregion
        
        #region Fields
        RuntimeConfiguration RuntimeConfiguration { get; }
        #endregion
    }
}