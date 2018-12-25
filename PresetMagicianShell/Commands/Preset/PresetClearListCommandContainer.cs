using Catel;
using Catel.Logging;
using Catel.MVVM;
using PresetMagicianShell.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
{
    // ReSharper disable once UnusedMember.Global
    public class PresetClearListCommandContainer : CommandContainerBase
    {
        private static readonly ILog _log = LogManager.GetCurrentClassLogger();

        private readonly IVstService _vstService;

        public PresetClearListCommandContainer(ICommandManager commandManager, IVstService vstService)
            : base(Commands.Preset.ClearList, commandManager)
        {
            Argument.IsNotNull(() => vstService);

            _vstService = vstService;
        }

        protected override void Execute(object parameter)
        {
            _vstService.PresetExportList.Clear();
        }
    }
}