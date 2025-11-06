using FastEndpoints;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using someServiceClient;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        // This will stop requiring a client certificate
        httpsOptions.ClientCertificateMode = ClientCertificateMode.NoCertificate;
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
    });

builder.Services.AddFastEndpoints();
builder.Services.AddGrpc();

AppContext.SetSwitch(
    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

builder.Services.AddGrpcClient<someService.someServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:5002");
});

var app = builder.Build();

app.UseAuthentication();

app.UseFastEndpoints();

app.Run();
