using Application.Abstractions.CQRS;
using Application.Contracts.Board;
using Application.Contracts.Persistence;
using Application.DTOs.ListCards;
using Application.Features.ListCards.Queries;
using Dapper;
using FluentResults;

namespace Application.Features.ListCards.QueryHandlers;

internal sealed class GetListCardByIdQueryHandler : IQueryHandler<GetListCardByIdQuery, ListCardResponse>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    public GetListCardByIdQueryHandler(IBoardRepository boardRepository,IDbConnectionFactory dbConnectionFactory)
    {
        _boardRepository = boardRepository;
        _dbConnectionFactory = dbConnectionFactory;
    }
    
    public async Task<Result<ListCardResponse>> Handle(GetListCardByIdQuery query, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithLists(query.BoardId, ct);
        if (board is null)
            return Result.Fail<ListCardResponse>("Board not found");
     
        var boardList = board.GetListById(query.BoardListId);
        if (boardList is null)
            return Result.Fail("Board list not found in this board");
        
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(ct);
        
        const string sql = """
                           SELECT "Id", @BoardId AS "BoardId", "BoardListId", "Title", "Description", "CompletedAt", "Position"
                           FROM "ListCards"
                           WHERE "Id" = @CardId AND "BoardListId" = @BoardListId
                           """;
        
        var listCard = await connection.QuerySingleOrDefaultAsync<ListCardResponse>(sql, new {BoardId = query.BoardId, BoardListId = query.BoardListId, CardId = query.CardId });
        if (listCard is null)
            return Result.Fail("Card not found in this list");
        
        return Result.Ok(listCard);
    }
}