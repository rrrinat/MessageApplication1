using MessageReceiverClient.Models;
using Microsoft.AspNetCore.Components;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MessageReceiverClient.Services
{
    public class WebSocketService : IAsyncDisposable
    {
        private readonly NavigationManager navigationManager;
        private readonly ILogger<WebSocketService> logger;
        private ClientWebSocket webSocket;
        private readonly Uri serverUri;
        private readonly SemaphoreSlim connectLock = new SemaphoreSlim(1, 1);

        public event Action<Message> OnMessageReceived;

        public WebSocketService(NavigationManager navigationManager, ILogger<WebSocketService> logger, string serverUri)
        {
            this.navigationManager = navigationManager;
            this.logger = logger;
            this.serverUri = new Uri(serverUri);
            this.webSocket = new ClientWebSocket();
        }

        public async Task ConnectAsync()
        {
            await connectLock.WaitAsync();
            try
            {
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.Connecting)
                    return;

                logger.LogInformation("Подключение к WebSocket серверу...");
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(serverUri, CancellationToken.None);
                logger.LogInformation("Подключение установлено.");

                _ = ReceiveMessages();
            }
            finally
            {
                connectLock.Release();
            }
        }

        private async Task ReceiveMessages()
        {
            var buffer = new byte[4096];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        logger.LogWarning("Соединение закрыто сервером.");
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрытие по запросу сервера", CancellationToken.None);
                        break;
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var message = JsonSerializer.Deserialize<Message>(messageJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (message != null)
                        {
                            logger.LogInformation($"Получено сообщение: {message.Text}, Sequence: {message.SequenceNumber}, Time: {message.Timestamp}");
                            OnMessageReceived?.Invoke(message);
                        }
                        else
                        {
                            logger.LogWarning("Некорректное сообщение.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при получении сообщений");
                await Task.Delay(TimeSpan.FromSeconds(5));
                await ConnectAsync();
            }
        }

        public async Task DisconnectAsync()
        {
            if (webSocket.State == WebSocketState.Open)
            {
                logger.LogInformation("Отключение от WebSocket сервера...");
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрытие по запросу клиента", CancellationToken.None);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
            webSocket.Dispose();
            connectLock.Dispose();
        }
    }
}
