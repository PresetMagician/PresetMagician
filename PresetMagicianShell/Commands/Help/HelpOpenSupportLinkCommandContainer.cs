using System.Diagnostics;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
{
    // ReSharper disable once UnusedMember.Global
    public class HelpOpenSupportLinkCommandContainer : CommandContainerBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public HelpOpenSupportLinkCommandContainer(ICommandManager commandManager)
            : base(Commands.Help.OpenSupportLink, commandManager)
        {
            
        }

        protected override void Execute (object parameter)
        {
            base.Execute(parameter);
            Process.Start(Settings.Links.Support);
        }
    }
}