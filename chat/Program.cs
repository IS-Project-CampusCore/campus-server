using Chat;
using Chat.Implementation;
using System.Reflection;
using commons.Database;
using commons.EventBase;
using commons.RequestBase;
using MassTransit;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using System.Net;
using usersServiceClient;

var builder = WebApplication.CreateBuilder(args);

Console.Clear();

builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext();
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddMassTransit(x =>
{
    var myAssembly = Assembly.GetExecutingAssembly();

    x.AddConsumers(myAssembly);

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.RegisterConsumers(context, myAssembly);
    });
});

builder.Services.AddSingleton<IScopedMessagePublisher, ScopedMessagePublisher>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ChatService).Assembly));

string connectionString = builder.Configuration["MongoDB:ConnectionString"]!;
string databaseName = builder.Configuration["MongoDB:DatabaseName"]!;

builder.Services.AddMongoDatabase(connectionString + databaseName, databaseName);

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServiceInterceptor>();
});

builder.Services.AddGrpcClient<usersService.usersServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:UsersService"];
    o.Address = new Uri(address!);
});

builder.Services.AddScoped<ServiceInterceptor>();

builder.Services.AddSingleton<ChatServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<ChatService>();

app.Run();