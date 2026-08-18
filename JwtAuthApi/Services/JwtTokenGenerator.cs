using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthApi.Services;

public class JwtTokenGenerator
{
    private readonly string _secretKey;
    private readonly string _audience;
    private readonly string _issuer;
    private readonly int _expiryInMinutes;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                     ?? configuration["JwtSettings:SecretKey"]
                     ?? throw new InvalidOperationException("JWT Secret Key is missing!");

        _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
                  ?? configuration["JwtSettings:Issuer"] 
                  ?? "JwtAuthApi";

        _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
                    ?? configuration["JwtSettings:Audience"] 
                    ?? "JwtAuthApiUsers";

        var expiryStr = Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") 
                        ?? configuration["JwtSettings:ExpiryInMinutes"];

        _expiryInMinutes = int.TryParse(expiryStr, out var expiry) ? expiry : 15;
    }

    public string GenerateAccessToken(string username, string role = "User")
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expiryInMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}