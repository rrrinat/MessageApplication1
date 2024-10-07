using Newtonsoft.Json;
using NLog;

namespace MessageHistoryClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Клиент для отображения истории сообщений за последние 10 минут");

            //var apiUrl = "http://localhost:5009/api/messages";
            var apiUrl = Environment.GetEnvironmentVariable("SERVER_URL") ?? "http://localhost:5009/api/messages";


            using (HttpClient client = new HttpClient())
            {
                while (true)
                {
                    try
                    {
                        DateTime endTime = DateTime.UtcNow;
                        DateTime startTime = endTime.AddMinutes(-10);

                        // Форматирование дат в ISO 8601
                        string startTimeStr = startTime.ToString("o");
                        string endTimeStr = endTime.ToString("o");

                        string requestUrl = $"{apiUrl}?startTime={Uri.EscapeDataString(startTimeStr)}&endTime={Uri.EscapeDataString(endTimeStr)}";

                        logger.Info("Запрос к API: {0}", requestUrl);

                        HttpResponseMessage response = await client.GetAsync(requestUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = await response.Content.ReadAsStringAsync();
                            var messages = JsonConvert.DeserializeObject<List<Message>>(jsonResponse);

                            if (messages != null)
                            {
                                logger.Info("Получено {0} сообщений за последние 10 минут:", messages.Count);

                                foreach (var message in messages)
                                {
                                    logger.Info("[{0}] ({1}) {2}", message.Timestamp.ToLocalTime(), message.SequenceNumber, message.Text);
                                }
                            }
                            else
                            {
                                logger.Warn("Не удалось получить сообщения.");
                            }
                        }
                        else
                        {
                            string errorResponse = await response.Content.ReadAsStringAsync();
                            logger.Error("Ошибка при запросе к API. Статус код: {0}", response.StatusCode);
                            logger.Error("Ответ сервера: {0}", errorResponse);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Произошла ошибка: {0}", ex.Message);
                    }

                    logger.Info("Обновление через 10 минут или нажмите Space для выхода...");

                    for (int i = 0; i < 600; i++)
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Spacebar)
                            {
                                return;
                            }
                        }
                        await Task.Delay(1000);
                    }
                }
            }

            logger.Info("Клиент завершил работу.");
        }
    }
}