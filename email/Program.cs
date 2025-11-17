using commons;
using email;
using email.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServiceInterceptor>();
});
builder.Services.AddScoped<ServiceInterceptor>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<EmailServiceImplementation>();
var app = builder.Build();

app.MapGrpcService<sendEmailMessage>();
app.MapGet("/", () => "gRPC service 'email' is running.");

app.Run();