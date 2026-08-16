using JwtAuthApi.Models;
using Microsoft.AspNetCore.Identity;

namespace JwtAuthApi.Services;

public class UserService
{
    private readonly PasswordHasher<User> _passwordHasher = new();
    private static readonly List<User> Users = new();

    public UserService()
    {
        if (!Users.Any())
        {
            var adminUser = new User { Username = "admin", Role = "Admin" };
            adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, "admin123");

            var normalUser = new User { Username = "john", Role = "User" };
            normalUser.PasswordHash = _passwordHasher.HashPassword(normalUser, "user123");

            Users.Add(adminUser);
            Users.Add(normalUser);
        }
    }

    public User? ValidateCredentials(string username, string password)
    {
        var user = Users.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (user is null) return null;

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        return result == PasswordVerificationResult.Success ? user : null;
    }

    public bool SaveRefreshToken(string username, string refreshToken, DateTime expiryTime)
    {
        var user = Users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        if (user is null) return false;

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = expiryTime;
        return true;
    }

    public User? RotateRefreshToken(string oldRefreshToken, string newRefreshToken, DateTime newExpiryTime)
    {
        var user = Users.FirstOrDefault(u => 
            u.RefreshToken == oldRefreshToken && 
            u.RefreshTokenExpiryTime > DateTime.UtcNow);

        if (user is null) return null;

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = newExpiryTime;

        return user;
    }
}