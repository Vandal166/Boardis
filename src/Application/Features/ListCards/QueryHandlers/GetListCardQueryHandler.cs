using Application.Abstractions.CQRS;
using Application.Contracts.Board;
using Application.Contracts.Persistence;
using Application.DTOs.ListCards;
using Application.Features.ListCards.Queries;
using Dapper;
using FluentResults;

namespace Application.Features.ListCards.QueryHandlers;

internal sealed class GetListCardQueryHandler : IQueryHandler<GetListCardsQuery, List<ListCardResponse>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    public GetListCardQueryHandler(IBoardRepository boardRepository, IDbConnectionFactory dbConnectionFactory)
    {
        _boardRepository = boardRepository;
        _dbConnectionFactory = dbConnectionFactory;
    }
    
    public async Task<Result<List<ListCardResponse>>> Handle(GetListCardsQuery query, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithLists(query.BoardId, ct);
        if (board is null)
            return Result.Fail<List<ListCardResponse>>("Board not found");
        
        var boardList = board.GetListById(query.BoardListId);
        if (boardList is null)
            return Result.Fail<List<ListCardResponse>>("Board list not found in this board");

        using var connection = await _dbConnectionFactory.CreateConnectionAsync(ct);
        
        const string sql = """
                           SELECT "Id", @BoardId AS "BoardId", "BoardListId", "Title", "Description", "CompletedAt", "Position"
                           FROM "ListCards"
                           WHERE "BoardListId" = @BoardListId
                           ORDER BY "Position" ASC
                           """;
        
        var listCard = await connection.QueryAsync<ListCardResponse>(sql, new {BoardId = query.BoardId, BoardListId = query.BoardListId });

        return Result.Ok(listCard.AsList());
    }
}