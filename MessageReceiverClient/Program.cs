using Newtonsoft.Json;
using NLog;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace MessageReceiverClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();

            logger.Info("Клиент для получения сообщений через WebSocket");
            logger.Info("Подключение к серверу...");

            var serverUriEnv = Environment.GetEnvironmentVariable("SERVER_WEBSOCKET_URL");

            if (string.IsNullOrEmpty(serverUriEnv))
            {
                logger.Error("Переменная окружения 'SERVER_WEBSOCKET_URL' не задана.");
                return; // Или выбросить исключение, если необходимо
            }

            if (!Uri.TryCreate(serverUriEnv, UriKind.Absolute, out Uri serverUri))
            {
                logger.Error("Переменная окружения 'SERVER_WEBSOCKET_URL' содержит некорректный URI: {0}", serverUriEnv);
                return; // Или выбросить исключение, если необходимо
            }

            using (ClientWebSocket ws = new ClientWebSocket())
            {
                try
                {
                    await ws.ConnectAsync(serverUri, CancellationToken.None);
                    logger.Info("Подключение установлено.");

                    var receiving = ReceiveMessages(ws);

                    await receiving;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Ошибка: {0}", ex.Message);
                }
            }

            logger.Info("Клиент завершил работу.");
        }

        static async Task ReceiveMessages(ClientWebSocket ws)
        {
            var logger = LogManager.GetCurrentClassLogger();

            var buffer = new byte[1024 * 4];

            while (ws.State == WebSocketState.Open)
            {
                try
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрытие по запросу сервера", CancellationToken.None);
                        logger.Info("Соединение закрыто сервером.");
                        break;
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var message = JsonConvert.DeserializeObject<Message>(messageJson);

                        if (message != null)
                        {
                            logger.Info("[{0}] ({1}) {2}", message.Timestamp.ToLocalTime(), message.SequenceNumber, message.Text);
                        }
                        else
                        {
                            logger.Warn("Получено некорректное сообщение.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Ошибка при получении сообщения: {0}", ex.Message);
                    break;
                }
            }
        }
    }
}
