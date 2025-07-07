using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;

namespace RT_NET.Api;

public class MessagesHub : Hub
{
    private readonly IDistributedCache _cache;

    public MessagesHub(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task SendMessage(string name, string text)
    {
        var messagesJson = await _cache.GetStringAsync("messages");
        List<Message> messages = [];

        if (!string.IsNullOrEmpty(messagesJson))
        {
            messages = JsonSerializer.Deserialize<List<Message>>(messagesJson)!;
        }

        var id = Guid.CreateVersion7().ToString();
        messages.Add(new Message
        {
            Id = id,
            Text = text,
            Name = name,
            InstanceId = Instance.Id
        });

        await _cache.SetStringAsync("messages", JsonSerializer.Serialize(messages));

        await Clients.All.SendAsync("ReceiveMessage", id, name, text, Instance.Id);
    }
}