using Newtonsoft.Json;
using NLog;
using System.Text;

namespace MessageSenderClient;

class Program
{
    static async Task Main(string[] args)
    {
        var logger = LogManager.GetCurrentClassLogger();
        logger.Info("Клиент для отправки сообщений через REST API");
        logger.Info("Введите 'exit' для завершения работы.");

        //string apiUrl = "http://localhost:5009/api/messages";
        string apiUrl = Environment.GetEnvironmentVariable("SERVER_URL") ?? "http://localhost:5009/api/messages";
        using (HttpClient client = new HttpClient())
        {
            int sequenceNumber = 1;

            while (true)
            {
                logger.Info("Введите текст сообщения: ");
                string input = Console.ReadLine();

                if (input.ToLower() == "exit")
                    break;

                if (string.IsNullOrWhiteSpace(input))
                {
                    logger.Warn("Текст сообщения не может быть пустым.");
                    continue;
                }

                if (input.Length > 128)
                {
                    logger.Warn("Текст сообщения не может превышать 128 символов.");
                    continue;
                }

                var message = new Message
                {
                    Text = input,
                    SequenceNumber = sequenceNumber++
                };

                var json = JsonConvert.SerializeObject(message);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        logger.Info("Сообщение успешно отправлено.");
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        logger.Error($"Ошибка при отправке сообщения. Статус код: {response.StatusCode}");
                        logger.Error($"Ответ сервера: {errorResponse}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Произошла ошибка: {ex.Message}");
                }
            }
        }

        logger.Info("Клиент завершил работу.");
    }
}