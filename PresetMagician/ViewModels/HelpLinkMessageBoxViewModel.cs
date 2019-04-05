// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBoxViewModel.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using System;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Reflection;
using Catel.Services;
using Orchestra.Services;
using PresetMagician.Core.Services;

namespace PresetMagician.ViewModels
{
    public enum DontMode
    {
        NONE,
        DONT_SHOW_AGAIN,
        DONT_ASK_AGAIN,
        DONT_CUSTOM
    }
    
    public class HelpLinkMessageBoxViewModel : ViewModelBase
    {
        private readonly IMessageService _messageService;
        private readonly IClipboardService _clipboardService;
        private readonly GlobalService _globalService;
        private readonly DataPersisterService _dataPersisterService;

        

        #region Constructors

        public HelpLinkMessageBoxViewModel(IMessageService messageService, IClipboardService clipboardService,
            GlobalService globalService, DataPersisterService dataPersisterService)
        {
            Argument.IsNotNull(() => messageService);
            Argument.IsNotNull(() => clipboardService);

            _messageService = messageService;
            _clipboardService = clipboardService;
            _globalService = globalService;
            _dataPersisterService = dataPersisterService;

            CopyToClipboard = new Command(OnCopyToClipboardExecute);

            OkCommand = new TaskCommand(OnOkCommandExecuteAsync);
            YesCommand = new TaskCommand(OnYesCommandExecuteAsync);
            NoCommand = new TaskCommand(OnNoCommandExecuteAsync);
            CancelCommand = new TaskCommand(OnCancelCommandExecuteAsync);
            EscapeCommand = new TaskCommand(OnEscapeCommandExecuteAsync);

            Result = MessageResult.None;
        }

        #endregion

        #region Properties

        public string Message { get; set; }

        public string HelpLink { get; set; }

        #region Dont stuff

        public DontMode DontMode { get; set; } = DontMode.NONE;
        public string DontText { get; set; }
        public string DontId { get; set; }
        public bool ShowDont { get; set; }
        public bool DontResult { get; set; }
        private bool _saveDont = true;
        #endregion
        
       


        public string FinalHelpLink
        {
            get { return Settings.Links.HelpLink + HelpLink + "?version=" + _globalService.PresetMagicianVersion; }
        }

        protected override Task InitializeAsync()
        {
           
            if (DontMode != DontMode.NONE)
            {
                ShowDont = true;

                if (DontMode != DontMode.DONT_CUSTOM && DontId == null)
                {
                    throw new ArgumentException("Missing DontId");
                }
            }

            if (DontMode == DontMode.DONT_ASK_AGAIN && Button == MessageButton.OK)
            {
                throw new ArgumentException($"Can't apply DONT_ASK_AGAIN to {Button}");
            }
            
            if (DontMode == DontMode.DONT_SHOW_AGAIN && Button != MessageButton.OK)
            {
                throw new ArgumentException($"Can apply DONT_SHOW_AGAIN only to {MessageButton.OK}, got {Button}");
            }


            return base.InitializeAsync();
        }
        
        

        public bool HasHelpLink
        {
            get
            {
                if (string.IsNullOrWhiteSpace(HelpLink))
                {
                    return false;
                }

                return true;
            }
        }

        public MessageResult Result { get; set; }

        public MessageButton Button { get; set; }

        public MessageImage Icon { get; set; }

        #endregion

        public void SetTitle(string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                Title = title;
                return;
            }

            var assembly = AssemblyHelper.GetEntryAssembly();
            Title = assembly.Title();
        }

        protected override async Task CloseAsync()
        {
            if (Result == MessageResult.None)
            {
                switch (Button)
                {
                    case MessageButton.OK:
                        Result = MessageResult.OK;
                        break;

                    case MessageButton.OKCancel:
                        Result = MessageResult.Cancel;
                        break;

                    case MessageButton.YesNoCancel:
                        Result = MessageResult.Cancel;
                        break;
                }
            }

            await base.CloseAsync();
        }

        #region Commands

        public Command CopyToClipboard { get; private set; }

        private void OnCopyToClipboardExecute()
        {
            var text = _messageService.GetAsText(Message, Button);

            _clipboardService.CopyToClipboard(text);
        }

        private void SetDontResult(MessageResult result)
        {
            if (!_saveDont)
            {
                return;
            }

            if (!DontResult)
            {
                return;
            }
            
            switch (DontMode)
            {
                case DontMode.DONT_ASK_AGAIN:
                    _globalService.SetRememberMyChoiceResult(DontId, result);
                    _dataPersisterService.SaveRememberMyChoiceResults();
                    break;
                case DontMode.DONT_SHOW_AGAIN:
                    _globalService.DontShowAgainDialogs.Add(DontId);
                    _dataPersisterService.SaveDontShowAgainDialogs();
                    break;
                case DontMode.DONT_CUSTOM:
                case DontMode.NONE:
                    break;
               
                        
            }
        }
        public TaskCommand OkCommand { get; private set; }

        private async Task OnOkCommandExecuteAsync()
        {
            if (Button == MessageButton.OK || Button == MessageButton.OKCancel)
            {
                Result = MessageResult.OK;

                SetDontResult(Result);
                
               

                await CloseViewModelAsync(null);
            }
        }

        public TaskCommand YesCommand { get; private set; }

        private async Task OnYesCommandExecuteAsync()
        {
            if (Button == MessageButton.YesNo || Button == MessageButton.YesNoCancel)
            {
                Result = MessageResult.Yes;
                SetDontResult(Result);
                await CloseViewModelAsync(null);
            }
        }

        public TaskCommand NoCommand { get; private set; }

        private async Task OnNoCommandExecuteAsync()
        {
            if (Button == MessageButton.YesNo || Button == MessageButton.YesNoCancel)
            {
                Result = MessageResult.No;
                SetDontResult(Result);
                await CloseViewModelAsync(null);
            }
        }

        public TaskCommand CancelCommand { get; private set; }

        private async Task OnCancelCommandExecuteAsync()
        {
            if (Button == MessageButton.YesNoCancel || Button == MessageButton.OKCancel)
            {
                Result = MessageResult.Cancel;
                SetDontResult(Result);
                await CloseViewModelAsync(null);
            }
        }

        public TaskCommand EscapeCommand { get; private set; }

        private async Task OnEscapeCommandExecuteAsync()
        {
            _saveDont = false;
            switch (Button)
            {
                case MessageButton.OK:
                    await OnOkCommandExecuteAsync();
                    break;

                case MessageButton.OKCancel:
                    await OnCancelCommandExecuteAsync();
                    break;

                case MessageButton.YesNo:
                    break;

                case MessageButton.YesNoCancel:
                    await OnCancelCommandExecuteAsync();
                    break;
            }
        }

        #endregion
    }
}