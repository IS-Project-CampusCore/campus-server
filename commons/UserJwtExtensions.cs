using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace commons;
public record UserJwt
{
    public string Id { get; set; } = string.Empty;
    public required string Name { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required string Role { get; set; } = "guest";

    public bool IsVerified = false;
}

public static class UserJwtExtensions
{
    public const string IdClaim = JwtRegisteredClaimNames.Sub;
    public const string RoleClaim = ClaimTypes.Role;
    public const string NameClaim = JwtRegisteredClaimNames.Name;
    public const string EmailClaim = JwtRegisteredClaimNames.Email;
    public const string IsVerifiedClaim = JwtRegisteredClaimNames.EmailVerified;

    private static readonly JwtSecurityTokenHandler s_jwtHandler = new();

    public static IEnumerable<Claim> ToClaims(this UserJwt jwt)
    {
        return [
            new Claim(IdClaim, jwt.Id),
            new Claim(RoleClaim, jwt.Role.ToString().ToLowerInvariant() ?? "guest"),
            new Claim(NameClaim, jwt.Name),
            new Claim(EmailClaim, jwt.Email),
            new Claim(IsVerifiedClaim, jwt.IsVerified.ToString().ToLowerInvariant())
        ];
    }

    public static UserJwt FromClaims(IEnumerable<Claim> claims)
    {
        return new UserJwt
        {
            Id = GetClaims(claims, IdClaim),
            Role = GetClaims(claims, RoleClaim),
            Name = GetClaims(claims, NameClaim),
            Email = GetClaims(claims, EmailClaim),
            IsVerified = GetClaims(claims, IsVerifiedClaim) == "true"
        };
    }

    public static UserJwt FromBearerToken(string token)
    {
        var claimPrincipal = s_jwtHandler.ReadJwtToken(token);
        return FromClaims(claimPrincipal.Claims);
    }

    public static string GetClaims(IEnumerable<Claim> claims, string claimType)
    {
        return claims.Single(c => c.Type == claimType).Value;
    }
}
