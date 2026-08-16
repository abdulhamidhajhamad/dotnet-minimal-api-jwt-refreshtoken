namespace JwtAuthApi.Models;

public record LoginRequest(string Username, string Password);

public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
public record RefreshTokenRequest(string RefreshToken);