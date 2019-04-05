// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using System.Threading.Tasks;
using System.Windows.Input;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;

namespace PresetMagician.Services
{
    public class AdvancedMessageService : MessageService, IAdvancedMessageService
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IDispatcherService _dispatcherService;
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly GlobalService _globalService;

        #endregion

        #region Constructors

        public AdvancedMessageService(IDispatcherService dispatcherService, IUIVisualizerService uiVisualizerService,
            IViewModelFactory viewModelFactory, ILanguageService languageService, GlobalService globalService)
            : base(dispatcherService, languageService)
        {
            Argument.IsNotNull(() => dispatcherService);
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => viewModelFactory);

            _dispatcherService = dispatcherService;
            _uiVisualizerService = uiVisualizerService;
            _viewModelFactory = viewModelFactory;
            _globalService = globalService;
        }

        #endregion

        public Task<MessageResult> ShowErrorAsync(string message, string caption = "", string helpLink = "")
        {
            return ShowAsync(message, caption, helpLink, MessageButton.OK, MessageImage.Error);
        }

        public Task<MessageResult> ShowWarningAsync(string message, string caption = "", string helpLink = "")
        {
            return ShowAsync(message, caption, helpLink, MessageButton.OK, MessageImage.Warning);
        }

        public Task<MessageResult> ShowInformationAsync(string message, string caption = "", string helpLink = "")
        {
            return ShowAsync(message, caption, helpLink, MessageButton.OK, MessageImage.Information);
        }

        public Task<MessageResult> ShowAsync(string message, string caption = "", string helpLink = null,
            MessageButton button = MessageButton.OK, MessageImage icon = MessageImage.None)
        {
            var tcs = new TaskCompletionSource<MessageResult>();

#pragma warning disable AvoidAsyncVoid
            _dispatcherService.BeginInvoke(async () =>
#pragma warning restore AvoidAsyncVoid
            {
                var previousCursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = null;

                var vm = _viewModelFactory.CreateViewModel<HelpLinkMessageBoxViewModel>(null, null);

                vm.Message = message;
                vm.Button = button;
                vm.Icon = icon;
                vm.HelpLink = helpLink;

                vm.SetTitle(caption);

                await _uiVisualizerService.ShowDialogAsync(vm);

                Mouse.OverrideCursor = previousCursor;

                tcs.TrySetResult(vm.Result);
            });

            return tcs.Task;
        }
        
        public Task<(MessageResult result, bool dontChecked)> ShowCustomRememberMyChoiceDialogAsync(string message,
            string caption = "", string helpLink = null, MessageButton button = MessageButton.OK,
            MessageImage icon = MessageImage.None, string dontAskAgainText = "")
        {
            Argument.IsNotNullOrWhitespace("message", message);

            var tcs = new TaskCompletionSource<(MessageResult result, bool dontChecked)>();

           
#pragma warning disable AvoidAsyncVoid
            _dispatcherService.BeginInvoke(async () =>
#pragma warning restore AvoidAsyncVoid
            {
                var previousCursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = null;

                var vm = _viewModelFactory.CreateViewModel<HelpLinkMessageBoxViewModel>(null, null);

                vm.Message = message;
                vm.Button = button;
                vm.Icon = icon;
                vm.HelpLink = helpLink;
                vm.DontMode = DontMode.DONT_CUSTOM;

                if (string.IsNullOrWhiteSpace(dontAskAgainText))
                {
                    dontAskAgainText = "Don't ask again";
                }

                vm.DontText = dontAskAgainText;

                vm.SetTitle(caption);

                await _uiVisualizerService.ShowDialogAsync(vm);

                Mouse.OverrideCursor = previousCursor;

                tcs.TrySetResult((vm.Result, vm.DontResult));
            });

            return tcs.Task;
        }

        public Task<MessageResult> ShowRememberMyChoiceDialogAsync(string message,string rememberMyChoiceId,
            string caption = "", string helpLink = null, MessageButton button = MessageButton.OK,
            MessageImage icon = MessageImage.None, string dontAskAgainText = "")
        {
            Argument.IsNotNullOrWhitespace("message", message);

            var tcs = new TaskCompletionSource<MessageResult>();

            if (_globalService.RememberMyChoiceResults.ContainsKey(rememberMyChoiceId))
            {
                tcs.TrySetResult(_globalService.RememberMyChoiceResults[rememberMyChoiceId]);
                return tcs.Task;
            }

#pragma warning disable AvoidAsyncVoid
            _dispatcherService.BeginInvoke(async () =>
#pragma warning restore AvoidAsyncVoid
            {
                var previousCursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = null;

                var vm = _viewModelFactory.CreateViewModel<HelpLinkMessageBoxViewModel>(null, null);

                vm.Message = message;
                vm.Button = button;
                vm.Icon = icon;
                vm.HelpLink = helpLink;
                vm.DontMode = DontMode.DONT_ASK_AGAIN;
                vm.DontId = rememberMyChoiceId;

                if (string.IsNullOrWhiteSpace(dontAskAgainText))
                {
                    dontAskAgainText = "Remember my choice";
                }

                vm.DontText = dontAskAgainText;

                vm.SetTitle(caption);

                await _uiVisualizerService.ShowDialogAsync(vm);

                Mouse.OverrideCursor = previousCursor;

                tcs.TrySetResult(vm.Result);
            });

            return tcs.Task;
        }
        
        public Task<MessageResult> ShowAsyncWithDontShowAgain(string message,string dontShowAgainId,
            string caption = "", string helpLink = null,
            MessageImage icon = MessageImage.None, string dontText = "")
        {
            Argument.IsNotNullOrWhitespace("message", message);

            var tcs = new TaskCompletionSource<MessageResult>();

            if (_globalService.DontShowAgainDialogs.Contains(dontShowAgainId))
            {
                tcs.TrySetResult(MessageResult.OK);
                return tcs.Task;
            }

#pragma warning disable AvoidAsyncVoid
            _dispatcherService.BeginInvoke(async () =>
#pragma warning restore AvoidAsyncVoid
            {
                var previousCursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = null;

                var vm = _viewModelFactory.CreateViewModel<HelpLinkMessageBoxViewModel>(null, null);

                vm.Message = message;
                vm.Button = MessageButton.OK;
                vm.Icon = icon;
                vm.HelpLink = helpLink;
                vm.DontMode = DontMode.DONT_SHOW_AGAIN;
                vm.DontId = dontShowAgainId;

                if (string.IsNullOrWhiteSpace(dontText))
                {
                    dontText = "Don't show again";
                }

                vm.DontText = dontText;

                vm.SetTitle(caption);

                await _uiVisualizerService.ShowDialogAsync(vm);

                Mouse.OverrideCursor = previousCursor;

                tcs.TrySetResult(vm.Result);
            });

            return tcs.Task;
        }
    }
}