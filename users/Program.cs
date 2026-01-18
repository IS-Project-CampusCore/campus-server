using commons.Database;
using commons.EventBase;
using commons.RequestBase;
using emailServiceClient;
using excelServiceClient;
using MassTransit;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using System.Net;
using users;
using users.Implementation;
using System.Reflection;

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

string connectionString = builder.Configuration["MongoDB:ConnectionString"]!;
string databaseName = builder.Configuration["MongoDB:DatabaseName"]!;

builder.Services.AddMongoDatabase(connectionString + databaseName, databaseName);

builder.Services.AddMassTransit(x =>
{
    var myAssembly = Assembly.GetExecutingAssembly();

    x.AddConsumers(myAssembly);

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.RegisterConsumers(context, myAssembly, "chat");
    });
});

builder.Services.AddSingleton<IScopedMessagePublisher, ScopedMessagePublisher>();

builder.Services.AddGrpcClient<emailService.emailServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:EmailService"];
    o.Address = new Uri(address!);
});

builder.Services.AddGrpcClient<excelService.excelServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:ExcelService"];
    o.Address = new Uri(address!);
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(UsersService).Assembly);
    cfg.LicenseKey = builder.Configuration["MediatR:LicenseKey"];
});

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServiceInterceptor>();
});

builder.Services.AddScoped<ServiceInterceptor>();

builder.Services.AddSingleton<IUsersServiceImplementation, UsersServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<UsersService>();

app.Run();