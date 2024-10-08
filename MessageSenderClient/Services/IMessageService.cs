using MessageSenderClient.Models;

namespace MessageSenderClient.Services
{
    public interface IMessageService
    {
        Task<bool> SendMessageAsync(Message message);
    }
}
