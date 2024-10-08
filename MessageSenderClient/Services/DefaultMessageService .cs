using MessageSenderClient.Models;
using System.Text;
using System.Text.Json;

namespace MessageSenderClient.Services
{
    public class DefaultMessageService : IMessageService
    {
        private readonly HttpClient httpClient;

        public DefaultMessageService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<bool> SendMessageAsync(Message message)
        {
            var json = JsonSerializer.Serialize(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("/api/messages", content);
            return response.IsSuccessStatusCode;
        }
    }
}
