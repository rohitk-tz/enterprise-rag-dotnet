using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

/// <summary>
/// Web-only authentication/authorization registration (JWT bearer + Clerk).
/// Kept separate from <see cref="DependencyInjection.AddInfrastructure"/> so that non-web hosts
/// (e.g. Workers) can depend on Infrastructure for database access without dragging in
/// ASP.NET Core JWT-bearer/HttpContext-specific types.
/// </summary>
public static class WebAuthDependencyInjection
{
    public static IServiceCollection AddWebAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Clerk:Authority"];
                options.MapInboundClaims = false; // keep "sub" as "sub", matching server/auth.py's payload["sub"]
                options.TokenValidationParameters.ValidateAudience = false;
            });
        services.AddAuthorization();

        return services;
    }
}
