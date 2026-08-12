namespace DefaultNamespace;

public class User
{
    public string username { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public string role { get; set; } = "User";
    
    public string? RefreshToken { get; set; }
    public dateTime? RefreshTokenExpires { get; set; }
}