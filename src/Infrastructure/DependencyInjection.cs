using Application.Contracts;
using Domain.Contracts;
using Infrastructure.Data;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Board;
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
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddScoped<IBoardRepository, BoardRepository>();
        
        return services;
    }
}