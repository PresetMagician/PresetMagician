using Catel.MVVM;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class DeveloperSetCatelLoggingCommandContainer : CommandContainerBase
    {
        public DeveloperSetCatelLoggingCommandContainer(ICommandManager commandManager)
            : base(Commands.Developer.SetCatelLogging, commandManager)
        {
        }

        protected override void Execute(object parameter)
        {
            App.SetCatelLogging((bool) parameter);
        }
    }
}