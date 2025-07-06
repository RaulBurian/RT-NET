var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.RT_NET_Api>("Api");
api.WithHttpEndpoint(5000);
api.WithHttpsEndpoint(5001);
api.WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

var frontend = builder.AddNpmApp("frontend", "../RT-NET.Frontend", "dev");
frontend.WithHttpEndpoint(3000, 5173);
frontend.WithReference(api);

builder.Build().Run();