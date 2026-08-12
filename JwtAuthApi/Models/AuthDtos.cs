namespace DefaultNamespace;

public class AuthDtos
{
    public record LoginReques(string username, string password);

    public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
    public record RefreshTokenRequest(string AccessToken, string RefreshToken);}