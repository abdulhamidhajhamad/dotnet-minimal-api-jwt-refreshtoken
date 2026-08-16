using JwtAuthApi.Models;
using JwtAuthApi.Services;

namespace JwtAuthApi.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/login", (LoginRequest request, UserService userService, JwtTokenGenerator jwtGenerator) =>
        {
            
            var user = userService.ValidateCredentials(request.Username, request.Password);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            var accessToken = jwtGenerator.GenerateAccessToken(user.Username, user.Role);
            var refreshToken = jwtGenerator.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(30);

            userService.SaveRefreshToken(user.Username, refreshToken, refreshTokenExpiry);

            return Results.Ok(new AuthResponse(accessToken, refreshToken, refreshTokenExpiry));
        });
        app.MapPost("/refresh-token", (RefreshTokenRequest request, UserService userService, JwtTokenGenerator jwtGenerator) =>
        {
            var newRefreshToken = jwtGenerator.GenerateRefreshToken();
            var newExpiry = DateTime.UtcNow.AddMinutes(30);
            var user = userService.RotateRefreshToken(request.RefreshToken, newRefreshToken, newExpiry);
            if (user is null)
            {
                return Results.Unauthorized();
            }
            var newAccessToken = jwtGenerator.GenerateAccessToken(user.Username, user.Role);

            return Results.Ok(new AuthResponse(newAccessToken, newRefreshToken, newExpiry));
        });
    }
}