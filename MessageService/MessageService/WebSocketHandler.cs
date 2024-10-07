using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace MessageService
{
    public static class WebSocketHandler
    {
        private static ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();
        private static ILogger _logger;

        public static void ConfigureLogger(ILogger logger)
        {
            _logger = logger;
        }

        public static async Task Handle(HttpContext context, WebSocket webSocket)
        {
            var socketId = context.Connection.Id;
            _sockets.TryAdd(socketId, webSocket);
            _logger?.LogInformation($"WebSocket connection established: {socketId}");

            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            _sockets.TryRemove(socketId, out _);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _logger?.LogInformation($"WebSocket connection closed: {socketId}");
        }

        public static async Task SendMessageToAllAsync(string message)
        {
            foreach (var socket in _sockets.Values)
            {
                if (socket.State == WebSocketState.Open)
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    _logger?.LogInformation("Message sent to WebSocket client.");
                }
            }
        }
    }
}