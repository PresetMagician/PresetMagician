using System.Diagnostics;
using Catel.Logging;
using Catel.MVVM;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class HelpOpenChatLinkCommandContainer : CommandContainerBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public HelpOpenChatLinkCommandContainer(ICommandManager commandManager)
            : base(Commands.Help.OpenChatLink, commandManager)
        {
        }

        protected override void Execute(object parameter)
        {
            base.Execute(parameter);
            Process.Start(Settings.Links.Chat);
        }
    }
}