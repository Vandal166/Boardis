using System.Security.Claims;
using Application.Contracts.Keycloak;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Web.API;

public static class DependencyInjection
{
    public static IServiceCollection AddWebAPI(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection keycloakSettings = configuration.GetSection("Keycloak");
        
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // cookies for session management
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        })
        .AddCookie(options =>
        {
            options.Cookie.Name = "BoardisAuth";
            options.ExpireTimeSpan = TimeSpan.FromHours(2); // cookie expires after 2 hours
            options.SlidingExpiration = true; // renews the cookie on each request and extends it by the ExpireTimeSpan
            
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // non-https for dev
            
            options.LoginPath = "/api/auth/login"; // redirect here if unauthenticated
            options.LogoutPath = "/api/auth/logout";
            
            options.AccessDeniedPath = "/api/auth/access-denied";
            
            
            options.Events.OnValidatePrincipal = async context =>
            {
                if (context.Principal?.Identity?.IsAuthenticated != true)
                    return;
                
                var lastValidated = context.Properties.IssuedUtc;

                // every 5 seconds we check if the user is still valid
                if (lastValidated == null || DateTimeOffset.UtcNow - lastValidated > TimeSpan.FromSeconds(5))
                {
                    var keycloakUserService = context.HttpContext.RequestServices.GetRequiredService<IKeycloakUserService>();
                    var userId = context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    
                    if (!string.IsNullOrEmpty(userId)) // if the userid is not empty meaning Authenticated
                    {
                        // we call Keycloak admin API to get the latest user state
                        var updatedUser = await keycloakUserService.GetUserByIdAsync(Guid.TryParse(userId, out var id) ? id : Guid.Empty);

                        if (updatedUser.ValueOrDefault == null) // if the user is not found/disabled then a change occurred on Keycloak side
                        {
                            // signing them out immediately and revoking sessions
                            context.RejectPrincipal();
                            await context.HttpContext.SignOutAsync();
                            await keycloakUserService.RevokeUserSessionAsync(Guid.TryParse(userId, out  _) ? id : Guid.Empty);
                            return;
                        }
                        context.ShouldRenew = true;
                    }
                }
            };
        })
        .AddJwtBearer(options => // JWT for API access
        {
            options.Authority = "http://web.keycloak:8081/auth/realms/BoardisRealm";
            options.Audience = keycloakSettings["Api:Audience"];
            options.RequireHttpsMetadata = false; // for dev
            options.MetadataAddress = "http://web.keycloak:8081/auth/realms/BoardisRealm/.well-known/openid-configuration";
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                // Override to match the token's actual issuer (external URL)
                ValidIssuer = keycloakSettings["Authority"], // "http://localhost/auth/realms/BoardisRealm"
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                NameClaimType = "preferred_username", // just mapping the 'preferred_username' claim to User.Identity.Name
                RoleClaimType = "realm_access.roles"
            };
            
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = ctx =>
                {
                    var token = ctx.Request.Headers.Authorization.FirstOrDefault()?.Split(' ').Last();
                    if (token != null)
                    {
                        var header = token.Split('.')[0];
                        var padded = header + new string('=', (4 - header.Length % 4) % 4);
                        var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
                        Console.WriteLine($"Token header: {json}");
                    }
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = ctx =>
                {
                    Console.WriteLine($"Auth failed: {ctx.Exception?.GetType().Name}: {ctx.Exception?.Message}");
                    if (ctx.Exception is SecurityTokenSignatureKeyNotFoundException)
                        Console.WriteLine("Signature key not found in JWKS.");
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    Console.WriteLine($"OnChallenge error: {context.Error}, description: {context.ErrorDescription}");
                    return Task.CompletedTask;
                }
            };
        })
        .AddOpenIdConnect(options =>
        {
            options.Authority = "http://web.keycloak:8081/auth/realms/BoardisRealm";
            options.ClientId = keycloakSettings["Web:ClientId"];
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.SaveTokens = true; // Save access/refresh and ID tokens in auth cookie
            options.GetClaimsFromUserInfoEndpoint = true; // Fetch additional claims
            
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            
            options.CallbackPath = "/api/auth/callback"; // This endpoint processes the authentication response and completes the sign-in
            options.SignedOutCallbackPath = "/api/auth/signout-callback";
            options.RequireHttpsMetadata = false; // for dev
            options.SkipUnrecognizedRequests = true;
            options.MetadataAddress = "http://web.keycloak:8081/auth/realms/BoardisRealm/.well-known/openid-configuration";
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                // Override to external issuer
                ValidIssuer = keycloakSettings["Authority"],
                NameClaimType = "preferred_username",
                RoleClaimType = "realm_access.roles"
            };
            
            options.Events = new OpenIdConnectEvents
            {
                OnRedirectToIdentityProvider = context =>
                {
                    var redirectUri = keycloakSettings["Web:RedirectUri"];
                    context.ProtocolMessage.RedirectUri = redirectUri?.ToLowerInvariant();
                    context.Properties.RedirectUri = redirectUri?.ToLowerInvariant(); // ensuring state is tied to redirect
                    context.Properties.IsPersistent = true; // make the cookie persistent across browser sessions meaning it will be saved even after closing the browser
                    return Task.CompletedTask;
                },
            };
            
        });
        
        return services;
    }
}