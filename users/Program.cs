using Serilog;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using users.Services;
using commons;

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

builder.Services.AddScoped<ServiceInterceptor>();

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServiceInterceptor>();
});

var app = builder.Build();

app.MapGrpcService<usersMessage>();

app.Run();