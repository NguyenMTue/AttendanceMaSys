using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AttendanceMaSys.Application.Common.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace AttendanceMaSys.Infrastructure.Services;

public class TokenService : ITokenService
{
    private const string SecretKey = "AttendanceMaSysSecretKeyForJwtAuthenticationTokensMustBeLongEnough!12345";

    public string GenerateToken(string userId, string email, string role, string? department, Guid? employeeId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(SecretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };

        if (!string.IsNullOrEmpty(department))
        {
            claims.Add(new Claim("Department", department));
        }

        if (employeeId.HasValue)
        {
            claims.Add(new Claim("EmployeeId", employeeId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
