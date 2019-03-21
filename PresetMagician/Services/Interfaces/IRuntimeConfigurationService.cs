using PresetMagician.Core.Models;

namespace PresetMagician.Services.Interfaces
{
    public interface IRuntimeConfigurationService
    {
        #region Methods

        void Load();
        void Save();
        void LoadConfiguration();

        void SaveConfiguration();

        #endregion

        #region Fields

        RuntimeConfiguration EditableConfiguration { get; }

        #endregion

        void CreateEditableConfiguration();
        void ApplyEditableConfiguration();
        bool IsConfigurationValueEqual(object left, object right);
    }
}