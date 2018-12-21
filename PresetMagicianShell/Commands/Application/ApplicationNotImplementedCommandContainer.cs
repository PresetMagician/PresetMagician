using System.Threading.Tasks;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
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

        protected override async Task ExecuteAsync(object parameter)
        {
            base.Execute(parameter);
        }
    }
}