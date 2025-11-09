using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using SomeService;
using SomeService.Services;
using System.Net;
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
        .WriteTo.Seq(seqUrl!);
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddScoped<ServiceInterceptor>();
builder.Services.AddSingleton<ServiceImplementation>();

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServiceInterceptor>();
});

var app = builder.Build();

app.MapGrpcService<DoSomethingMessage>();

app.Run();

