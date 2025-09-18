using System.Security.Claims;
using Application.Contracts.Communication;
using Application.Contracts.Keycloak;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Web.API.Common;
using Web.API.Communication.Services;

namespace Web.API;

public static class DependencyInjection
{
    public static IServiceCollection AddWebAPI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();
        
        services.AddScoped<IBoardHubNotifier, BoardHubNotifier>();
        services.AddScoped<INotificationNotifier, NotificationNotifier>();
        
        
        IConfigurationSection keycloakSettings = configuration.GetSection("Keycloak");
        
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        })
        .AddJwtBearer(options => // JWT validation for API access
        {
            options.Authority = "http://web.keycloak:8081/auth/realms/BoardisRealm"; // Internal Keycloak for metadata fetch
            options.Audience = keycloakSettings["Api:Audience"];
            options.RequireHttpsMetadata = false; // For dev; set to true in prod
            options.MetadataAddress = "http://web.keycloak:8081/auth/realms/BoardisRealm/.well-known/openid-configuration";
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                // Override to match the token's actual issuer
                ValidIssuer = keycloakSettings["Authority"], //"http://localhost/auth/realms/BoardisRealm"
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                NameClaimType = "preferred_username", // mapping 'preferred_username' to User.Identity.Name
                RoleClaimType = "realm_access.roles"
            };

            // this is triggered per-request on valid tokens.
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var principal = context.Principal;
                    if (principal?.Identity?.IsAuthenticated != true)
                        return;

                    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                        return;

                    var keycloakUserService = context.HttpContext.RequestServices.GetRequiredService<IKeycloakUserService>();
                    var id = Guid.TryParse(userId, out var parsedId) ? parsedId : Guid.Empty;

                    var updatedUser = await keycloakUserService.GetUserByIdAsync(id);
                    if (updatedUser.ValueOrDefault == null) // if user is disabled/deleted in Keycloak
                    {
                        // rejecting the session in IDP
                        context.Fail("User no longer valid in identity provider.");
                        await keycloakUserService.RevokeUserSessionAsync(id);
                    }
                },
                // Crucial: Handle access_token from query for WebSockets/SSE
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    // Apply only to hub paths
                    if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/boardHub") || path.StartsWithSegments("/generalNotificationHub")))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });
        
        return services;
    }
}