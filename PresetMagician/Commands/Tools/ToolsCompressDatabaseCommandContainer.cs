using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Threading;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;

namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class ToolsCompressDatabaseCommandContainer : ThreadedApplicationNotBusyCommandContainer
    {
        private readonly IApplicationService _applicationService;
        private readonly IDatabaseService _databaseService;

        public ToolsCompressDatabaseCommandContainer(ICommandManager commandManager,
            IApplicationService applicationService, IDatabaseService databaseService,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.Tools.CompressDatabase, commandManager, runtimeConfigurationService)
        {
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => databaseService);
            _applicationService = applicationService;
            _databaseService = databaseService;
        }

        protected override async Task ExecuteThreaded(object parameter)
        {
            var compress = (bool) parameter;
            var operationDescription = "Compressing Presets";
            if (!compress)
            {
                operationDescription = "Decompressing Presets";
            }

            _applicationService.StartApplicationOperation(this, operationDescription, 0);
            var total = new Progress<int>(t => { _applicationService.SetApplicationOperationTotalItems(t); });
            var token = _applicationService.GetApplicationOperationCancellationSource().Token;

            var progress = new Progress<int>(percent =>
            {
                _applicationService.UpdateApplicationOperationStatus(percent,
                    $"{operationDescription} {percent} / {_runtimeConfigurationService.ApplicationState.ApplicationBusyTotalItems}");
            });


            await TaskHelper.Run(async () =>
                await _databaseService.Context.CompressOrDecompressPresets(compress, token, progress, total));

            _applicationService.StopApplicationOperation(operationDescription + " complete");
            base.Execute(parameter);
        }
    }
}