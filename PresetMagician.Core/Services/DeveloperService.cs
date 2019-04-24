using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Anotar.Catel;
using Catel.Services;

namespace PresetMagician.Core.Services
{
    public class DeveloperService
    {
        private readonly GlobalService _globalService;
        private readonly IMessageService _messageService;
        
        public DeveloperService(GlobalService globalService, IMessageService messageService)
        {
            _globalService = globalService;
            _messageService = messageService;
        }

        public void StartHexEditor(string fileName)
        {
            if (string.IsNullOrWhiteSpace(_globalService.RuntimeConfiguration.HexEditorExecutable))
            {
                _messageService.ShowErrorAsync(
                    $"No hex editor specified");
                return;
            }
            
            if (!File.Exists(_globalService.RuntimeConfiguration.HexEditorExecutable))
            {
                _messageService.ShowErrorAsync(
                    $"{_globalService.RuntimeConfiguration.HexEditorExecutable}: No such file or directory");
                return;
            }

            var argument = _globalService.RuntimeConfiguration.HexEditorArguments;

            if (string.IsNullOrWhiteSpace(argument))
            {
                argument = "%s";
            }

            if (!argument.Contains("%s"))
            {
                _messageService.ShowErrorAsync(
                    $"Placeholder %s is missing in hex editor argument {argument}");
                return;
            }

            try
            {
                argument = argument.Replace("%s",
                    Regex.Replace(fileName, @"(\\+)$", @"$1$1"));
                
                LogTo.Debug($"Starting Hex Editor {_globalService.RuntimeConfiguration.HexEditorExecutable} with argument {argument}");
                var processStartInfo = new ProcessStartInfo(_globalService.RuntimeConfiguration.HexEditorExecutable)
                {
                    Arguments = argument,
                    UseShellExecute = true
                };


                var proc = new Process {StartInfo = processStartInfo};

                proc.Start();
            }
            catch (Exception e)
            {
                _messageService.ShowErrorAsync(
                    $"Unable to start hex editor {_globalService.RuntimeConfiguration.HexEditorExecutable} with "+
                    $"argument {argument} because of {e.GetType().FullName}: {e.Message}");
            }
        }

    }
}