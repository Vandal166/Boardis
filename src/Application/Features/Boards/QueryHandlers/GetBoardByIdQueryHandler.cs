using Application.Abstractions.CQRS;
using Application.Contracts.Persistence;
using Application.DTOs.Boards;
using Application.Features.Boards.Queries;
using Dapper;
using FluentResults;

namespace Application.Features.Boards.QueryHandlers;

internal sealed class GetBoardByIdQueryHandler : IQueryHandler<GetBoardByIdQuery, BoardResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    public GetBoardByIdQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<BoardResponse>> Handle(GetBoardByIdQuery query, CancellationToken ct = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(ct);
        
        const string sql = """
                           SELECT "Id", "Title", "Description", "WallpaperImageId", "Visibility"
                           FROM "Boards"
                           WHERE "Id" = @BoardId
                           ORDER BY "CreatedAt" DESC
                           """;
        
        var boardResponse = await connection.QuerySingleOrDefaultAsync<BoardResponse>(sql, new { BoardId = query.BoardId });
        if (boardResponse is null)
            return Result.Fail("BoardNotFound");
        
        return Result.Ok(boardResponse);
    }
}