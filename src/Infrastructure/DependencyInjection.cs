using Application.Contracts;
using Application.Contracts.Board;
using Application.Contracts.Keycloak;
using Application.Contracts.Persistence;
using Infrastructure.Data;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Board;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Services.Keycloak;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BoardisDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Boardis_DB"));
        });
       
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });
    
        services.AddScoped<DomainEventInterceptor>();
        
        services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(configuration));

        services.AddHttpContextAccessor();
        services.AddHttpClient();
        
        services.AddScoped<IKeycloakService, KeycloakService>();
        services.AddScoped<IKeycloakUserService, KeycloakUserService>();
        
        services.AddMemoryCache();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddScoped<IBoardRepository, BoardRepository>();
        
        return services;
    }
}