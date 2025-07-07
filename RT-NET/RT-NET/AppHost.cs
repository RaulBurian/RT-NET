var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithRedisCommander();

var api = builder.AddProject<Projects.RT_NET_Api>("Api");
api.WithHttpEndpoint(5000, isProxied: false);
api.WithHttpsEndpoint(5001, isProxied: false);
api.WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");
api.WithEnvironment("ASPNETCORE_URLS", "http://localhost:5000;https://localhost:5001");
api.WithReference(redis);

var frontend = builder.AddNpmApp("frontend", "../RT-NET.Frontend", "dev");
frontend.WithHttpEndpoint(3000, 5173);
frontend.WithReference(api);

builder.Build().Run();