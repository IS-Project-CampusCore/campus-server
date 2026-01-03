using Chat;
using Chat.Implementation;
using Chat.Services;
using commons;
using MassTransit;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using System.Net;
using usersServiceClient;

var builder = WebApplication.CreateBuilder(args);

var seqUrl = builder.Configuration["Logging:SeqUrl"];
builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console()
        .WriteTo.Seq(seqUrl ?? "http://localhost:5341");
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
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ChatService).Assembly));

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