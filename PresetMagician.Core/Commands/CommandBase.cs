using System.Threading.Tasks;
using Catel.IoC;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.Core.Commands
{
    public abstract class CommandBase
    {
        protected readonly IServiceLocator ServiceLocator;
        protected readonly GlobalService GlobalService;
        protected readonly GlobalFrontendService GlobalFrontendService;
        protected readonly IApplicationService ApplicationService;
        
        public CommandBase(IServiceLocator serviceLocator)
        {
            ServiceLocator = serviceLocator;
            ApplicationService = serviceLocator.ResolveType<IApplicationService>();
            GlobalService = serviceLocator.ResolveType<GlobalService>();
            GlobalFrontendService = serviceLocator.ResolveType<GlobalFrontendService>();
        }

        public abstract Task ExecuteAsync();

    }
}