using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Infrastructure.Identity;
using BareqAlNaqool.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BareqAlNaqool.Infrastructure.Security;

public class JwtTokenService(
    IOptions<JwtSettings> options,
    UserManager<ApplicationUser> userManager)
{
    private readonly JwtSettings _settings = options.Value;

    public async Task<string> CreateTokenAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("displayName", user.FullName),
            new("isGuest", user.IsGuest.ToString().ToLowerInvariant())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GetMode(ApplicationUser user, IList<string> roles)
    {
        if (user.IsGuest || roles.Contains(AppRoles.Guest))
        {
            return "guest";
        }

        return "member";
    }
}
