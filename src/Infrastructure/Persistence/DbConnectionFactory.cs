using System.Data;
using Application.Contracts.Persistence;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Infrastructure.Persistence;

internal sealed class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken ct = default)
    {
        var connection = new NpgsqlConnection(_configuration.GetConnectionString("Boardis_DB"));
        await connection.OpenAsync(ct);
        
        return connection;
    }
}