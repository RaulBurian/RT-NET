using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace RT_NET.Api;

public class WebSocketHandler
{
    private readonly WebSocketConnectionManager _connectionManager;
    private readonly IDistributedCache _cache;

    public WebSocketHandler(WebSocketConnectionManager connectionManager, IDistributedCache cache)
    {
        _connectionManager = connectionManager;
        _cache = cache;
    }

    public async Task HandleConnection(WebSocket webSocket)
    {
        var connectionId = _connectionManager.AddSocket(webSocket);
        var buffer = new byte[4096];

        try
        {
            var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                var receivedMessage = System.Text.Encoding.UTF8.GetString(
                    buffer, 0, receiveResult.Count);

                var messageRequest = JsonSerializer.Deserialize<MessageRequest>(receivedMessage)!;
                var message = new Message
                {
                    Id = Guid.CreateVersion7().ToString(),
                    Text = messageRequest.Text,
                    Name = messageRequest.Name
                };

                await StoreMessageInCache(message);

                var messageJson = JsonSerializer.Serialize(message);
                await _connectionManager.SendMessageToAllAsync(messageJson);

                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }
        finally
        {
            await _connectionManager.RemoveSocket(connectionId);
        }
    }

    private async Task StoreMessageInCache(Message message)
    {
        var messagesJson = await _cache.GetStringAsync("messages");
        List<Message> messages = [];

        if (!string.IsNullOrEmpty(messagesJson))
        {
            messages = JsonSerializer.Deserialize<List<Message>>(messagesJson)!;
        }

        messages.Add(message);

        var updatedMessagesJson = JsonSerializer.Serialize(messages);
        await _cache.SetStringAsync("messages", updatedMessagesJson);
    }
}