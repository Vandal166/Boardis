using Application.Contracts;
using Application.Contracts.Board;
using Application.Contracts.Keycloak;
using Application.Contracts.Persistence;
using Infrastructure.Data;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Board;
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
    
        services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(configuration));
        
        services.AddHttpContextAccessor();
        services.AddHttpClient();
        
        services.AddScoped<IKeycloakService, KeycloakService>();
        services.AddScoped<IKeycloakUserService, KeycloakUserService>();
        
        services.AddMemoryCache();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddScoped<IBoardRepository, BoardRepository>();
        
        services.AddScoped<IBoardMemberRepository, BoardMemberRepository>();
        services.Decorate<IBoardMemberRepository, CachedBoardMemberRepository>();

        services.AddScoped<IBoardMemberPermissionRepository, BoardMemberPermissionRepository>();
        services.Decorate<IBoardMemberPermissionRepository, CachedBoardMemberPermissionRepository>();
        
        services.AddScoped<IBoardListRepository, BoardListRepository>();
        services.Decorate<IBoardListRepository, CachedBoardListRepository>();
        
        services.AddScoped<IListCardRepository, ListCardRepository>();
        //services.Decorate<IListCardRepository, CachedListCardRepository>(); // this will now use the cached version
        
        return services;
    }
}