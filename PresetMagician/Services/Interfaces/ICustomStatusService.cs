using Fluent;
using Orchestra.Services;

namespace PresetMagician.Services.Interfaces
{
    public interface ICustomStatusService : IStatusService
    {
        void Initialize(StatusBarItem statusBarItem);
    }
}