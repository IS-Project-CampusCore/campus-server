using excelServiceClient;
using FastEndpoints;
using http.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Serilog;
using usersServiceClient;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext();
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ClientCertificateMode = ClientCertificateMode.NoCertificate;
    });
});

builder.Services.AddGrpc();

builder.Services.AddGrpcClient<usersService.usersServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:UsersService"];
    o.Address = new Uri(address!);
});

builder.Services.AddGrpcClient<emailServiceClient.emailService.emailServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:EmailService"];
    o.Address = new Uri(address!);
});

builder.Services.AddGrpcClient<excelService.excelServiceClient>(o =>
{
    string? address = builder.Configuration["GrpcServices:ExcelService"];
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

app.UseFastEndpoints();

app.Run();
