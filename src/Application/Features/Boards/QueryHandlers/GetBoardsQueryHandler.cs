using Application.Abstractions.CQRS;
using Application.Contracts.Persistence;
using Application.DTOs.Boards;
using Application.Features.Boards.Queries;
using Dapper;
using FluentResults;

namespace Application.Features.Boards.QueryHandlers;

internal sealed class GetBoardsQueryHandler : IQueryHandler<GetBoardsQuery, List<BoardResponse>>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    public GetBoardsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<List<BoardResponse>>> Handle(GetBoardsQuery query, CancellationToken ct = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(ct);
        
        const string sql = """
                           SELECT "Id", "Title", "Description", "Visibility"
                           FROM "Boards"
                           LEFT JOIN "BoardMembers" ON "Boards"."Id" = "BoardMembers"."BoardId"
                           WHERE "BoardMembers"."UserId" = @UserId
                           ORDER BY "Boards"."CreatedAt" DESC
                           """;
        
        var boardResponses = await connection.QueryAsync<BoardResponse>(sql, new {UserId = query.RequestingUserId });

        return Result.Ok(boardResponses.AsList());
    }
}