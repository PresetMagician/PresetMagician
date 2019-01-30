using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel;
using Catel.Collections;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Models;
using PresetMagician.ProcessIsolation;
using PresetMagician.Services.Interfaces;
using SharedModels;

namespace PresetMagician.ViewModels
{
    public class VstWorkerPoolViewModel : ViewModelBase
    {
        private readonly IApplicationService _applicationService;

        public VstWorkerPoolViewModel(IApplicationService applicationService,
            ICommandManager commandManager)
        {
            Argument.IsNotNull(() => applicationService);
            _applicationService = applicationService;

            ProcessPool = _applicationService.NewProcessPool;
        }

   
        public NewProcessPool ProcessPool { get; }
    }
}