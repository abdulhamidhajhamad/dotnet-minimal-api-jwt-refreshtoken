namespace JwtAuthApi.Endpoints;

public static class ProtectedEndpoints
{
    public static void MapProtectedEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/protected", () => Results.Ok(new { Message = "You have accessed a secure endpoint!" }))
            .RequireAuthorization();

        app.MapGet("/admin", () => Results.Ok(new { Message = "Welcome to the Admin portal!" }))
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}