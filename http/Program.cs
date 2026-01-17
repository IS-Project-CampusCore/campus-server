using announcementsServiceClient;
using chatServiceClient;
using excelServiceClient;
using FastEndpoints;
using http.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using System.Net;
using usersServiceClient;
using gradesServiceClient;
using scheduleServiceClient;
using emailServiceClient;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

Console.Clear();

builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext();
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 8080, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

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

builder.Services.AddGrpcClient<chatService.chatServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:ChatService"];
    o.Address = new Uri(address!);
});

builder.Services.AddGrpcClient<announcementsService.announcementsServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:AnnouncementsService"];
    o.Address = new Uri(address!);
});

builder.Services.AddGrpcClient<gradesService.gradesServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:GradesService"];
    o.Address = new Uri(address!);
});

builder.Services.AddGrpcClient<scheduleService.scheduleServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:ScheduleService"];
    o.Address = new Uri(address!);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CampusAuthentication.SchemeName;
    options.DefaultChallengeScheme = CampusAuthentication.SchemeName;
}).AddScheme<AuthenticationSchemeOptions, CampusAuthentication>(
    CampusAuthentication.SchemeName,
    option => { }
);

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(CampusPolicy.AuthenticatedUser, CampusPolicy.AuthenticatedUserPolicy)
    .AddPolicy(CampusPolicy.UnverifiedUser, CampusPolicy.UnverifiedUserPolicy);

builder.Services.AddFastEndpoints();

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.UseFastEndpoints(c =>
{
    c.Serializer.Options.PropertyNamingPolicy = null;
});

app.Run();
