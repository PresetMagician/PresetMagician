using PresetMagician.Core.Data;

namespace PresetMagician.ViewModels
{
    public interface IModelTracker
    {
        IUserEditable GetTrackedModel();
    }
}