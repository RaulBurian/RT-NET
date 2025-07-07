using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;

namespace RT_NET.Api;

public class WebSocketConnectionManager
{
    public ConcurrentDictionary<string, WebSocket> Sockets { get; } = new();

    public string AddSocket(WebSocket socket)
    {
        var connectionId = Guid.NewGuid().ToString();
        Sockets.TryAdd(connectionId, socket);
        return connectionId;
    }

    public async Task RemoveSocket(string id)
    {
        if (Sockets.TryRemove(id, out var socket))
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Connection closed by the server",
                    CancellationToken.None);
            }
        }
    }

    public async Task SendMessageToAllAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        var messageBytes = JsonSerializer.SerializeToUtf8Bytes(message);
        var tasks = new List<Task>();
        var failedConnections = new List<string>();

        foreach (var pair in Sockets)
        {
            var socket = pair.Value;
            var connectionId = pair.Key;

            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    tasks.Add(socket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, cancellationToken));
                }
                catch (Exception)
                {
                    failedConnections.Add(connectionId);
                }
            }
            else
            {
                failedConnections.Add(connectionId);
            }
        }

        await Task.WhenAll(tasks);

        foreach (var connectionId in failedConnections)
        {
            await RemoveSocket(connectionId);
        }
    }
}