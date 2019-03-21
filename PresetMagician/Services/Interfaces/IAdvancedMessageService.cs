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

        Task<(MessageResult result, bool dontAskAgainChecked)> ShowAsyncWithDontAskAgain(string message,
            string caption = "", string helpLink = null, MessageButton button = MessageButton.OK,
            MessageImage icon = MessageImage.None, string dontAskAgainText = "");
    }
}