using Serilog;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using MyGrpcService.Services;
using commons;
using MyGrpcService;
using MyGrpcService.Implementation;

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

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MyGrpcServiceService).Assembly));

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServiceInterceptor>();
});
builder.Services.AddScoped<ServiceInterceptor>();

builder.Services.AddSingleton<MyGrpcServiceServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<MyGrpcServiceService>();

app.Run();