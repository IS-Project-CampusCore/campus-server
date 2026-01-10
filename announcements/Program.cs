using Serilog;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Announcements.Services;
using commons;
using Announcements;
using Announcements.Implementation;
using commons.RequestBase;

var builder = WebApplication.CreateBuilder(args);

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
    cfg.RegisterServicesFromAssembly(typeof(AnnouncementsService).Assembly);
    cfg.LicenseKey = builder.Configuration["MediatR:LicenseKey"];
});

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServiceInterceptor>();
});
builder.Services.AddScoped<ServiceInterceptor>();

builder.Services.AddSingleton<AnnouncementsServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<AnnouncementsService>();

app.Run();