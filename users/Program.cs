using Serilog;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using users.Services;
using commons;
using users;
using emailServiceClient;
using excelServiceClient;

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

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UsersService).Assembly));

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServiceInterceptor>();
});

builder.Services.AddScoped<ServiceInterceptor>();

builder.Services.AddSingleton<IUsersServiceImplementation, UsersServiceImplementation>();

var app = builder.Build();

app.MapGrpcService<UsersService>();

app.Run();