using announcements;
using announcements.Implementation;
using commons.Database;
using commons.RequestBase;
using emailServiceClient;
using usersServiceClient;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using System.Net;

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

string connectionString = builder.Configuration["MongoDB:ConnectionString"]!;
string databaseName = builder.Configuration["MongoDB:DatabaseName"]!;

builder.Services.AddMongoDatabase(connectionString + databaseName, databaseName);

builder.Services.AddGrpcClient<emailService.emailServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:EmailService"];
    o.Address = new Uri(address!);
});

builder.Services.AddGrpcClient<usersService.usersServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:UsersService"];
    o.Address = new Uri(address!);
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

builder.Services.AddSingleton<AnnouncementServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<AnnouncementsService>();

app.Run();