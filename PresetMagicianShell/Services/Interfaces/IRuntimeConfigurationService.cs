using System.Threading.Tasks;
using PresetMagicianShell.Models;

namespace PresetMagicianShell.Services.Interfaces
{
    public interface IRuntimeConfigurationService
    {
        #region Methods
        void Load();
        void Save(bool includeCaching = false);
        void LoadConfiguration();
        void LoadLayout();
        
        void ResetLayout();

        void SaveConfiguration(bool includeCaching = false);
        void SaveLayout();
        #endregion
        
        #region Fields
        RuntimeConfiguration RuntimeConfiguration { get; }
        ApplicationState ApplicationState { get; }
        RuntimeConfiguration EditableConfiguration { get; }

        #endregion

        void CreateEditableConfiguration();
        void ApplyEditableConfiguration();
        bool IsConfigurationValueEqual(object left, object right);
    }
}