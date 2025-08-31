using Application.Contracts;
using Application.Contracts.Keycloak;
using Domain.Contracts;
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

        services.AddHttpContextAccessor();
        services.AddHttpClient<IKeycloakService, KeycloakService>();
        services.AddHttpClient<IKeycloakUserService, KeycloakUserService>();
        services.AddHttpClient<IKeycloakRoleService, KeycloakRoleService>();
        services.AddMemoryCache();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<IBoardMemberRepository, BoardMemberRepository>();
        services.AddScoped<IBoardListRepository, BoardListRepository>();
        services.AddScoped<IListCardRepository, ListCardRepository>();
        
        return services;
    }
}