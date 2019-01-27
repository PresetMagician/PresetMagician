using Catel.Logging;
using Catel.MVVM;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class ApplicationNotImplementedCommandContainer : CommandContainerBase
    {
        private static readonly ILog _log = LogManager.GetCurrentClassLogger();

        public ApplicationNotImplementedCommandContainer(ICommandManager commandManager)
            : base(Commands.Application.NotImplemented, commandManager)
        {
        }

        protected override bool CanExecute(object parameter)
        {
            return false;
        }
    }
}