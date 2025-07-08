using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.OpenApi.Models;
using RT_NET.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi(opts =>
{
    opts.AddDocumentTransformer((document, _, _) =>
    {
        document.Servers.Clear();
        document.Servers.Add(new OpenApiServer
        {
            Url = builder.Configuration["ServerUrl"] ?? "http://localhost:5000"
        });

        return Task.CompletedTask;
    });
});
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddCors(opts =>
{
    opts.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.SetIsOriginAllowed(_ => true);
        policy.AllowCredentials();
    });
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["REDISCACHECONNSTR_redis"];
});
builder.Services.AddSignalR()
    .AddStackExchangeRedis(builder.Configuration["REDISCACHECONNSTR_redis"]!);

var app = builder.Build();

app.MapOpenApi();

app.UseCors();
app.UseStaticFiles();

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

app.MapPost("/messages", async (MessageRequest messageRequest, IDistributedCache cache, IHubContext<MessagesHub> hubContext) =>
{
    var message = new Message
    {
        Id = Guid.CreateVersion7().ToString(),
        Text = messageRequest.Text,
        Name = messageRequest.Name,
        InstanceId = Instance.Id
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

    await hubContext.Clients.All.SendAsync("ReceiveMessage", message.Name, message.Text);

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

app.MapGet("/health", () => Results.Ok()).ExcludeFromDescription();

app.MapHub<MessagesHub>("/messagesHub");

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

    [JsonPropertyName("instanceId")]
    public required string InstanceId { get; set; }
}

static class Instance
{
    public static string Id { get; } = Guid.NewGuid().ToString();
}