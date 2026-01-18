using campus;
using campus.Implementation;
using commons.Database;
using commons.RequestBase;
using emailServiceClient;
using excelServiceClient;
using MassTransit;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using System.Diagnostics;
using System.Net;
using usersServiceClient;
using commons.EventBase;
using System.Reflection;

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
        cfg.RegisterConsumers(context, myAssembly, "campus");
    });
});

string connectionString = builder.Configuration["MongoDB:ConnectionString"]!;
string databaseName = builder.Configuration["MongoDB:DatabaseName"]!;

builder.Services.AddMongoDatabase(connectionString + databaseName, databaseName);

builder.Services.AddGrpcClient<usersService.usersServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:UsersService"];
    o.Address = new Uri(address!);
});

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
    cfg.RegisterServicesFromAssembly(typeof(CampusService).Assembly);
    cfg.LicenseKey = builder.Configuration["MediatR:LicenseKey"];
});

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServiceInterceptor>();
});
builder.Services.AddScoped<ServiceInterceptor>();

builder.Services.AddSingleton<CampusServiceImplementation>();

var app = builder.Build();

try
{
    var serviceImpl = app.Services.GetRequiredService<CampusServiceImplementation>();

    string? filePath = builder.Configuration["CampusConfig:CampusExcelPath"];
    string? fileName = builder.Configuration["CampusConfig:CampusExcelName"];

    if (filePath is null || fileName is null)
    {
        return;
    }

    await serviceImpl.GenerateCampusAsync(filePath, fileName);
}
catch (Exception ex)
{
    Debug.WriteLine(ex);
    return;
}

app.MapGrpcService<CampusService>();

app.Run();