using Serilog;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using users;
using emailServiceClient;
using excelServiceClient;
using commons.RequestBase;
using commons.Database;
using users.Implementation;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Serilog
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext();
});

// 2. Configure Kestrel (HTTP/2 for gRPC)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// 3. Register Database (ADDED THIS BLOCK)
// This resolves the IDatabase dependency error
string connectionString = builder.Configuration["MongoDB:ConnectionString"]!;
string databaseName = builder.Configuration["MongoDB:DatabaseName"]!;

// Combining connection string and database name as seen in your Excel service
builder.Services.AddMongoDatabase(connectionString + databaseName, databaseName);

// 4. Register gRPC Clients
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

// 5. Register MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(UsersService).Assembly);
    cfg.LicenseKey = builder.Configuration["MediatR:LicenseKey"];
});

// 6. Register gRPC Server and Interceptors
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServiceInterceptor>();
});

builder.Services.AddScoped<ServiceInterceptor>();

// 7. Register Application Services
builder.Services.AddSingleton<IUsersServiceImplementation, UsersServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<UsersService>();

app.Run();