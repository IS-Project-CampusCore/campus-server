using commons.RequestBase;
using email;
using email.Implementation;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using System.Net;

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

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(EmailService).Assembly);
    cfg.LicenseKey = builder.Configuration["MediatR:LicenseKey"];
});

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServiceInterceptor>();
});
builder.Services.AddScoped<ServiceInterceptor>();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<EmailServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<EmailService>();

app.Run();