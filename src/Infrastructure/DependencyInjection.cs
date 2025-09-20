using Application.Contracts;
using Application.Contracts.Board;
using Application.Contracts.Keycloak;
using Application.Contracts.Media;
using Application.Contracts.Persistence;
using Azure.Storage.Blobs;
using Infrastructure.Data;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Board;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Persistence.Media;
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
        
        services.AddSingleton<IBlobStorage, BlobStorage>();
        services.AddSingleton(_ => new BlobServiceClient(configuration.GetConnectionString("BlobStorage")));
        
        services.AddScoped<DomainEventInterceptor>();
        
        services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(configuration));

        services.AddHttpContextAccessor();
        services.AddHttpClient();
        
        services.AddScoped<IKeycloakService, KeycloakService>();
        services.AddScoped<IKeycloakUserService, KeycloakUserService>();
        
        services.AddMemoryCache();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        

        return services;
    }
}