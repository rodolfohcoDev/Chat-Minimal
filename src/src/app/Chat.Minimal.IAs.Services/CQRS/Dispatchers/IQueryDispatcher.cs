using Chat.Minimal.IAs.Services.CQRS.Queries;

namespace Chat.Minimal.IAs.Services.CQRS.Dispatchers;

public interface IQueryDispatcher
{
    Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>;
}
