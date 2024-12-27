using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services;

public class JwtService {
    private readonly IConfiguration _configuration;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public JwtService(IConfiguration configuration) {
        _configuration = configuration;
        _tokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
            ),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    }

    public string GenerateToken(int userId, string username, string role) {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddMinutes(120);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken(string token, out ClaimsPrincipal? principal) {
        principal = null;

        try {
            var tokenHandler = new JwtSecurityTokenHandler();
            principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
            return true;
        } catch {
            return false;
        }
    }
}
