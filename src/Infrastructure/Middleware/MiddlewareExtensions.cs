using Microsoft.AspNetCore.Builder;

namespace Infrastructure.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseTokenValidation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TokenValidationMiddleware>();
    }
}