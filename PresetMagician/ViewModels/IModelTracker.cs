using Catel.Data;
using SharedModels;

namespace PresetMagician.ViewModels
{
    public interface IModelTracker
    {
        TrackableModelBase GetTrackedModel();
    }
}