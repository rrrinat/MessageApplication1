using MessageHistoryClient.Models;
using System.Text.Json;

namespace MessageHistoryClient.Services
{
    public class MessageService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<MessageService> logger;

        public MessageService(HttpClient httpClient, ILogger<MessageService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<List<Message>> GetMessagesAsync()
        {
            try
            {
                var endTime = DateTime.UtcNow;
                var startTime = endTime.AddMinutes(-10);
                var startTimeStr = Uri.EscapeDataString(startTime.ToString("o"));
                var endTimeStr = Uri.EscapeDataString(endTime.ToString("o"));

                var apiUrl = Environment.GetEnvironmentVariable("SERVER_URL") ?? "http://localhost:5009/api/messages";
                var requestUrl = $"{apiUrl}?startTime={startTimeStr}&endTime={endTimeStr}";

                logger.LogInformation("Запрос к API: {0}", requestUrl);

                var response = await httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var messages = JsonSerializer.Deserialize<List<Message>>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    logger.LogInformation("Получено {0} сообщений за последние 10 минут", messages?.Count ?? 0);

                    return messages ?? new List<Message>();
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    logger.LogError("Ошибка при запросе к API. Статус код: {0}", response.StatusCode);
                    logger.LogError("Ответ сервера: {0}", errorResponse);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Произошла ошибка: {0}", ex.Message);
            }

            return new List<Message>();
        }
    }
}
