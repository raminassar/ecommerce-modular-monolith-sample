using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq.Expressions;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;

namespace BuildingBlocks.Core.Messaging.MessagePersistence.InMemory;

public class InMemoryMessagePersistenceRepository : IMessagePersistenceRepository
{
    private readonly ConcurrentDictionary<Guid, StoreMessage> _messages = new();

    public Task AddAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(storeMessage, nameof(storeMessage));

        _messages.TryAdd<Guid, StoreMessage>(storeMessage.Id, storeMessage);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<StoreMessage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<StoreMessage> result = _messages.Select(x => x.Value).ToImmutableList();

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<StoreMessage>> GetByFilterAsync(
        Expression<Func<StoreMessage, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(predicate, nameof(predicate));

        IReadOnlyList<StoreMessage>
            result = _messages.Select(x => x.Value).Where(predicate.Compile()).ToImmutableList();

        return Task.FromResult(result);
    }

    public Task<StoreMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = _messages.FirstOrDefault(x => x.Key == id).Value;

        return Task.FromResult(result)!;
    }

    public Task<bool> RemoveAsync(StoreMessage storeMessage, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(storeMessage, nameof(storeMessage));

        var result = _messages.Remove(storeMessage.Id, out _);

        return Task.FromResult(result)!;
    }
}
