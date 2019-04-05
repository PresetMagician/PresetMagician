using System.Threading.Tasks;
using Catel.Services;

namespace PresetMagician.Services.Interfaces
{
    public interface IAdvancedMessageService : IMessageService
    {
        Task<MessageResult> ShowErrorAsync(string message, string caption = "", string helpLink = "");
        Task<MessageResult> ShowWarningAsync(string message, string caption = "", string helpLink = "");
        Task<MessageResult> ShowInformationAsync(string message, string caption = "", string helpLink = "");

        Task<MessageResult> ShowAsync(string message, string caption = "", string helpLink = null,
            MessageButton button = MessageButton.OK, MessageImage icon = MessageImage.None);


        Task<MessageResult> ShowRememberMyChoiceDialogAsync(string message,string rememberMyChoiceId,
            string caption = "", string helpLink = null, MessageButton button = MessageButton.OK,
            MessageImage icon = MessageImage.None, string dontAskAgainText = "");

        Task<MessageResult> ShowAsyncWithDontShowAgain(string message,string dontShowAgainId,
            string caption = "", string helpLink = null,
            MessageImage icon = MessageImage.None, string dontText = "");

        Task<(MessageResult result, bool dontChecked)> ShowCustomRememberMyChoiceDialogAsync(string message,
            string caption = "", string helpLink = null, MessageButton button = MessageButton.OK,
            MessageImage icon = MessageImage.None, string dontAskAgainText = "");
    }
}