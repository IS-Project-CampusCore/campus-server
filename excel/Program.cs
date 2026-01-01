using commons;
using commons.Database;
using excel;
using excel.Implementation;
using excel.Services;
using excelServiceClient;
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

string connectionString = builder.Configuration["MongoDB:ConnectionString"]!;
string databaseName = builder.Configuration["MongoDB:DatabaseName"]!;

builder.Services.AddMongoDatabase(connectionString + databaseName, databaseName);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ExcelService).Assembly));

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServiceInterceptor>();
});
builder.Services.AddScoped<ServiceInterceptor>();

builder.Services.AddSingleton<ExcelServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<ExcelService>();

app.Run();