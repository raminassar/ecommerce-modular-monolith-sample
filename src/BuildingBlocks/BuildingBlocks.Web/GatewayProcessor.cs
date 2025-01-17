using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.CQRS.Query;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Web.Module;

namespace BuildingBlocks.Web;

public class GatewayProcessor<TModule> : IGatewayProcessor<TModule>
    where TModule : class, IModuleDefinition
{
    private readonly IServiceProvider _serviceProvider;

    public GatewayProcessor(IServiceProvider serviceProvider)
    {
        // https://blog.stephencleary.com/2016/12/eliding-async-await.html
        var compositionRoot = CompositionRootRegistry.GetByModule<TModule>();
        _serviceProvider = compositionRoot?.ServiceProvider ?? serviceProvider;
    }

    public async Task ExecuteCommand<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        using var scope = _serviceProvider.CreateScope();
        var commandProcessor = scope.ServiceProvider.GetRequiredService<ICommandProcessor>();

        await commandProcessor.SendAsync(command, cancellationToken);
    }

    public async Task<TResult> ExecuteCommand<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
        where TResult : notnull
    {
        using var scope = _serviceProvider.CreateScope();
        var commandProcessor = scope.ServiceProvider.GetRequiredService<ICommandProcessor>();

        return await commandProcessor.SendAsync(command, cancellationToken);
    }

    public async Task ExecuteCommand(Func<ICommandProcessor, IMapper, Task> action)
    {
        using var scope = _serviceProvider.CreateScope();
        var commandProcessor = scope.ServiceProvider.GetRequiredService<ICommandProcessor>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        await action.Invoke(commandProcessor, mapper);
    }

    public async Task ExecuteCommand(Func<ICommandProcessor, Task> action)
    {
        await ExecuteCommand(async (processor, _) => await action(processor));
    }

    public async Task<T> ExecuteCommand<T>(Func<ICommandProcessor, Task<T>> action)
    {
        return await ExecuteCommand(async (processor, _) => await action(processor)).ConfigureAwait(false);
    }

    public async Task<T> ExecuteCommand<T>(Func<ICommandProcessor, IMapper, Task<T>> action)
    {
        using var scope = _serviceProvider.CreateScope();
        var commandProcessor = scope.ServiceProvider.GetRequiredService<ICommandProcessor>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        return await action.Invoke(commandProcessor, mapper);
    }

    public async Task Publish(Func<IBus, Task> action)
    {
        var bus = _serviceProvider.GetRequiredService<IBus>();
        await action(bus);
    }

    public async Task<TResult> ExecuteQuery<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
        where TResult : notnull
    {
        using var scope = _serviceProvider.CreateScope();
        var queryProcessor = scope.ServiceProvider.GetRequiredService<IQueryProcessor>();

        return await queryProcessor.SendAsync(query, cancellationToken);
    }

    public async Task<T> ExecuteQuery<T>(Func<IQueryProcessor, Task<T>> action)
    {
        return await ExecuteQuery(async (processor, _) => await action(processor))
            .ConfigureAwait(false);
    }

    public async Task<T> ExecuteQuery<T>(Func<IQueryProcessor, IMapper, Task<T>> action)
    {
        using var scope = _serviceProvider.CreateScope();
        var queryProcessor = scope.ServiceProvider.GetRequiredService<IQueryProcessor>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        return await action.Invoke(queryProcessor, mapper);
    }
}
