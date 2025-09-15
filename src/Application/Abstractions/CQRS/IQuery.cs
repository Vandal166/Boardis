namespace Application.Abstractions.CQRS;

/// <summary>
/// A query in CQRS represents a request to retrieve data without causing any side effects or changes to the system's state.
/// </summary>
/// <typeparam name="TResponse">The type of the response that the query will return.</typeparam>
public interface IQuery<TResponse>;


public interface ICacheableQuery
{
    string CacheKey { get; } // The unique key to identify the cached data
    bool BypassCache { get; }  // if true will skip cache and go to the source
    TimeSpan? AbsoluteExpiration { get; }
    TimeSpan? SlidingExpiration { get; }
}