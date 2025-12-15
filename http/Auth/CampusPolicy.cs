using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using commons;
using usersServiceClient;

namespace http.Auth;

public static class CampusPolicy
{
    public const string AuthenticatedUser = "Authenticated";
    public const string UnverifiedUser = "Unverified";

    public static AuthorizationPolicy AuthenticatedUserPolicy { get; } = new AuthorizationPolicyBuilder()
        .RequireClaim(UserJwtExtensions.IdClaim)
        .RequireClaim(UserJwtExtensions.IsVerifiedClaim, "true")
        .RequireAuthenticatedUser()
        .Build();

    public static AuthorizationPolicy UnverifiedUserPolicy { get; } = new AuthorizationPolicyBuilder()
        .RequireClaim(UserJwtExtensions.IdClaim)
        .RequireAuthenticatedUser()
        .Build();
}

public sealed class CampusAuthentication(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration config
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Bearer";
    private const string AuthHeader = "X-Campus-Bearer";

    private readonly IConfiguration _config = config;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? token = Context.Request.Headers[AuthHeader].SingleOrDefault();
        if (token is null)
        {
            string? authHeader = Context.Request.Headers.Authorization.SingleOrDefault();
            if (authHeader is not null && authHeader.Length > "Bearer ".Length)
            {
                token = authHeader["Bearer ".Length..];
            }
        }

        if (token is null)
        {
            return AuthenticateResult.Fail("Mising token");
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            tokenHandler.MapInboundClaims = false;

            var key = Encoding.ASCII.GetBytes(_config["SecretKey"] ?? "a_very_secret_key_that_must_be_long_and_complex");

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            var userId = principal.FindFirst(UserJwtExtensions.IdClaim)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return AuthenticateResult.Fail("Token missing User ID");
            }

            var userServiceClient = Context.RequestServices.GetRequiredService<usersService.usersServiceClient>();

            var user = await userServiceClient.GetUserByIdAsync(new UserIdRequest { Id = userId });

            if (user is null)
            {
                return AuthenticateResult.Fail("User no longer exists");
            }

            var identity = new ClaimsIdentity(
                principal.Claims,
                Scheme.Name,
                "name",
                "role"
            );

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (SecurityTokenExpiredException)
        {
            return AuthenticateResult.Fail("Token has expiered");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Auth failed:{ex.Message}", ex);
            return AuthenticateResult.Fail($"Auth failed:{ex.Message}");
        }
    }
}
