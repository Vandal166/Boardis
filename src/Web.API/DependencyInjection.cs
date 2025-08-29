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
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme; // use cookies to authenticate by default
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        })
        .AddCookie(options =>
        {
            options.Cookie.Name = "BoardisAuth";
            options.ExpireTimeSpan = TimeSpan.FromHours(2);
            options.SlidingExpiration = true; // renews the cookie on each request
            
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // for dev
            
            options.LoginPath = "/api/auth/login"; // redirect here if unauthenticated
            options.LogoutPath = "/api/auth/logout";
            
            options.AccessDeniedPath = "/api/auth/access-denied";
        })
        .AddJwtBearer(options =>
        {
            options.Authority = keycloakSettings["Authority"];
            options.Audience = keycloakSettings["Audience"];
            options.RequireHttpsMetadata = false; // for dev
            options.MetadataAddress = $"{keycloakSettings["Authority"]}/.well-known/openid-configuration";

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                NameClaimType = "preferred_username", // just mapping the 'preferred_username' claim to User.Identity.Name
                RoleClaimType = "realm_access.roles"
            };
        })
        .AddOpenIdConnect(options =>
        {
            options.Authority = keycloakSettings["Authority"];
            options.ClientId = keycloakSettings["ClientId"];
            options.ClientSecret = keycloakSettings["ClientSecret"];
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.SaveTokens = true; // Save access/refresh tokens
            options.GetClaimsFromUserInfoEndpoint = true; // Fetch additional claims
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.CallbackPath = "/api/auth/callback"; // This endpoint processes the authentication response and completes the sign-in
            options.SignedOutCallbackPath = "/api/auth/signout-callback";
            options.RequireHttpsMetadata = false; // for dev
            options.SkipUnrecognizedRequests = true;
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "preferred_username",
                RoleClaimType = "realm_access.roles"
            };
            
            options.Events = new OpenIdConnectEvents
            {
                OnRedirectToIdentityProvider = context =>
                {
                    var redirectUri = keycloakSettings["RedirectUri"];
                    context.ProtocolMessage.RedirectUri = redirectUri?.ToLowerInvariant();
                    context.Properties.RedirectUri = redirectUri?.ToLowerInvariant(); // ensuring state is tied to redirect
                    context.Properties.IsPersistent = true; // make the cookie persistent
                    return Task.CompletedTask;
                },
            };
            
        });

        
        return services;
    }
}