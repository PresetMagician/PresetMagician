using System.Threading.Tasks;
using PresetMagicianShell.Models;

namespace PresetMagicianShell.Services.Interfaces
{
    public interface IRuntimeConfigurationService
    {
        #region Methods
        void Load();
        void Save();
        void LoadConfiguration();
        void LoadLayout();
        
        void ResetLayout();

        void SaveConfiguration();
        void SaveLayout();
        #endregion
        
        #region Fields
        RuntimeConfiguration RuntimeConfiguration { get; }
        ApplicationState ApplicationState { get; }

        #endregion
    }
}