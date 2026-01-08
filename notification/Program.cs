using chatServiceClient;
using commons.EventBase;
using commons.RequestBase;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using notification.Auth;
using notification.Hubs;
using notification.Implementation;
using Notification;
using Notification.Implementation;
using Serilog;
using System.Net;
using System.Reflection;
using usersServiceClient;
using commons.SignalRBase;

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

    serverOptions.Listen(IPAddress.Any, 8081, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = ChatAuthentication.SchemeName;
    options.DefaultChallengeScheme = ChatAuthentication.SchemeName;
}).AddScheme<AuthenticationSchemeOptions, ChatAuthentication>(
    ChatAuthentication.SchemeName,
    option => { }
);

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(ChatPolicy.AuthenticatedUser, ChatPolicy.AuthenticatedUserPolicy);

builder.Services.AddMassTransit(x =>
{
    var myAssembly = Assembly.GetExecutingAssembly();

    x.AddConsumers(myAssembly);

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.RegisterConsumers(context, myAssembly);
    });
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(NotificationService).Assembly));

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServiceInterceptor>();
});

builder.Services.AddGrpcClient<chatService.chatServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:ChatService"];
    o.Address = new Uri(address!);
});

builder.Services.AddGrpcClient<usersService.usersServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:UsersService"];
    o.Address = new Uri(address!);
});

builder.Services.AddScoped<ServiceInterceptor>();

builder.Services.AddSingleton<NotificationServiceImplementation>();
builder.Services.AddSingleton<IConnectionMapping<ChatHub>, ConnectionMapping<ChatHub>>();
builder.Services.AddScoped<INotifier<ChatHub>, HubNotifier<ChatHub>>();

var app = builder.Build();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<NotificationService>();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();