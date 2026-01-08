using Serilog;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using users;
using emailServiceClient;
using excelServiceClient;
using commons.RequestBase;

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