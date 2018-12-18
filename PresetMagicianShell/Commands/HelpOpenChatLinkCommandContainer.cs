using System.Diagnostics;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
{
    public class HelpOpenChatLinkCommandContainer : CommandContainerBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public HelpOpenChatLinkCommandContainer(ICommandManager commandManager)
            : base(Commands.Help.OpenChatLink, commandManager)
        {
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            base.Execute(parameter);
            Process.Start(Settings.Links.Chat);
        }
    }
}