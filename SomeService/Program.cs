using Grpc.Net.Client;
using SomeService;
using Grpc.Net;
using SomeService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddSingleton<ServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<DoSomethingMessage>();

app.Run();

