using System.Diagnostics;
using Catel.Logging;
using Catel.MVVM;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class HelpOpenSupportLinkCommandContainer : CommandContainerBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public HelpOpenSupportLinkCommandContainer(ICommandManager commandManager)
            : base(Commands.Help.OpenSupportLink, commandManager)
        {
        }

        protected override void Execute(object parameter)
        {
            base.Execute(parameter);
            Process.Start(Settings.Links.Support);
        }
    }
}