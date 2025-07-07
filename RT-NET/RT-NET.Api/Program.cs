using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowAnyOrigin();
    });
});
builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = builder.Configuration.GetConnectionString("redis"); });

var app = builder.Build();

app.UseCors();
app.MapOpenApi();
app.UseSwaggerUI(opts =>
{
    opts.SwaggerEndpoint("/openapi/v1.json", "RT-NET API");
});

app.MapPost("/clear", async (IDistributedCache cache) => await cache.SetStringAsync("messages", "[]"));

app.MapGet("/messages", async (IDistributedCache cache) =>
{
    var messagesJson = await cache.GetStringAsync("messages");

    if (string.IsNullOrEmpty(messagesJson))
    {
        return Results.Ok(Array.Empty<Message>());
    }

    var messages = JsonSerializer.Deserialize<List<Message>>(messagesJson)!;
    return Results.Ok(messages);
});

app.MapPost("/messages", async (MessageRequest messageRequest, IDistributedCache cache) =>
{
    var message = new Message
    {
        Id = Guid.CreateVersion7().ToString(),
        Text = messageRequest.Text,
        Name = messageRequest.Name
    };

    var messagesJson = await cache.GetStringAsync("messages");
    List<Message> messages = [];

    if (!string.IsNullOrEmpty(messagesJson))
    {
        messages = JsonSerializer.Deserialize<List<Message>>(messagesJson)!;
    }

    messages.Add(message);

    var updatedMessagesJson = JsonSerializer.Serialize(messages);
    await cache.SetStringAsync("messages", updatedMessagesJson);

    return Results.Created($"/messages/{message.Id}", message);
});

app.MapGet("/messages/{id}", async (string id, IDistributedCache cache) =>
{
    var messagesJson = await cache.GetStringAsync("messages");

    if (string.IsNullOrEmpty(messagesJson))
    {
        return Results.NotFound($"Message with ID {id} not found");
    }

    var messages = JsonSerializer.Deserialize<List<Message>>(messagesJson)!;
    var message = messages.FirstOrDefault(m => m.Id == id);

    if (message == null)
    {
        return Results.NotFound($"Message with ID {id} not found");
    }

    return Results.Ok(message);
});

app.Run();

class MessageRequest
{
    [JsonPropertyName("text")]
    public required string Text { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }
}

class Message
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }
}