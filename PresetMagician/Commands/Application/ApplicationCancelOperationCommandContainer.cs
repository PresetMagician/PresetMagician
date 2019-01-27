using Catel;
using Catel.MVVM;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class ApplicationCancelOperationCommandContainer : CommandContainerBase
    {
        private readonly IApplicationService _applicationService;


        public ApplicationCancelOperationCommandContainer(ICommandManager commandManager,
            IApplicationService applicationService)
            : base(Commands.Application.CancelOperation, commandManager)
        {
            Argument.IsNotNull(() => applicationService);

            _applicationService = applicationService;
        }

        protected override void Execute(object parameter)
        {
            _applicationService.CancelApplicationOperation();
        }
    }
}