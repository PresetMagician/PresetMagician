using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Services;
using Portable.Licensing;
using PresetMagicianShell.Services;
using System.Linq;
using System.Windows.Forms;
using Syroot.Windows.IO;

namespace PresetMagicianShell.ViewModels
{
    public class RegistrationViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IOpenFileService _openFileService;
        private readonly ILicenseService _licenseService;
        private readonly IMessageService _messageService;

        public string SystemCode { get; }
        public bool ValidLicense { get; private set; } = false;
        
        public event EventHandler LicenseUpdated;

        
        public RegistrationViewModel(INavigationService navigationService, IOpenFileService openFileService, ILicenseService licenseService, IMessageService messageService)
        {
            Argument.IsNotNull(() => navigationService);
            Argument.IsNotNull(() => openFileService);
            Argument.IsNotNull(() => licenseService);
            Argument.IsNotNull(() => messageService);

            _navigationService = navigationService;
            _openFileService = openFileService;
            _licenseService = licenseService;
            _messageService = messageService;
            
            CloseApplication = new TaskCommand(OnCloseApplicationExecuteAsync);
            GetLicense = new TaskCommand(OnGetLicenseExecuteAsync);
            SelectLicenseFile= new TaskCommand(OnSelectLicenseFileExecuteAsync);
            SystemCode = LicenseService.SystemCodeInfo.getSystemInfo();
        }
        
        public TaskCommand CloseApplication { get; private set; }

        private async Task OnCloseApplicationExecuteAsync()
        {
            _navigationService.CloseApplication();

        }

        public TaskCommand GetLicense { get; set; }

        private async Task OnGetLicenseExecuteAsync()
        {
            Process.Start(Settings.Links.GetTrialLicense);
        }
        
        public TaskCommand SelectLicenseFile { get; private set; }

        private async Task OnSelectLicenseFileExecuteAsync()
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

                if (!validationErrors.Any())
                {
                    ValidLicense = true;
                    LicenseUpdated.SafeInvoke(this, EventArgs.Empty);
                }
                else
                {
                    Collection<string> errors = new Collection<string>();
                    
                    errors.Add("The License is not valid!"+Environment.NewLine);
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