using Grpc.Net;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using SomeService;
using SomeService.Services;
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
        .WriteTo.Seq(seqUrl!);
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddGrpc();

builder.Services.AddSingleton<ServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<DoSomethingMessage>();

app.Run();

