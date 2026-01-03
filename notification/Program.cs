using chatServiceClient;
using commons;
using MassTransit;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using notification.Hubs;
using notification.Implementation;
using Notification;
using Notification.Implementation;
using Notification.Services;
using Serilog;
using System.Net;

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

builder.Services.AddSignalR();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<MessageCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("gateway-chat-queue", e =>
        {
            e.ConfigureConsumer<MessageCreatedConsumer>(context);
        });
    });
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(NotificationService).Assembly));

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServiceInterceptor>();
});

builder.Services.AddGrpcClient<chatService.chatServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:ChatService"];
    o.Address = new Uri(address!);
});

builder.Services.AddScoped<ServiceInterceptor>();

builder.Services.AddSingleton<NotificationServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<NotificationService>();

app.MapHub<ChatHub>("/hubs/chat");

app.Run();