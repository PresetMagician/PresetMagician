using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Services.Interfaces;
using Syroot.Windows.IO;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class ToolsUpdateLicenseCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IOpenFileService _openFileService;
        private readonly ILicenseService _licenseService;
        private readonly IMessageService _messageService;

        public ToolsUpdateLicenseCommandContainer(ICommandManager commandManager, IServiceLocator serviceLocator)
            : base(Commands.Tools.UpdateLicense, commandManager, serviceLocator)
        {
            _openFileService = ServiceLocator.ResolveType<IOpenFileService>();
            _licenseService = ServiceLocator.ResolveType<ILicenseService>();
            _messageService = ServiceLocator.ResolveType<IMessageService>();
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            string downloadsPath = new KnownFolder(KnownFolderType.Downloads).Path;

            _openFileService.InitialDirectory = downloadsPath;
            _openFileService.Filter = "License Files (*.lic)|*.lic";
            _openFileService.FilterIndex = 1;

            if (await _openFileService.DetermineFileAsync())
            {
                //Get the path of specified file
                var filePath = _openFileService.FileName;

                var validationErrors = _licenseService.UpdateLicense(filePath);

                if (validationErrors.Any())
                {
                    Collection<string> errors = new Collection<string>();

                    errors.Add("The License is not valid!" + Environment.NewLine);
                    foreach (var validationError in validationErrors)
                    {
                        errors.Add($"{validationError.Message}");
                    }

                    await _messageService.ShowErrorAsync(String.Join(Environment.NewLine, errors));
                }
            }
        }
    }
}