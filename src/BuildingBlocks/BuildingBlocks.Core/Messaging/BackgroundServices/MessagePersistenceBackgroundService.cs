using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Abstractions.Types;
using BuildingBlocks.Core.Messaging.MessagePersistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Core.Messaging.BackgroundServices;

public class MessagePersistenceBackgroundService : IHostedService
{
    private readonly ILogger<MessagePersistenceBackgroundService> _logger;
    private readonly MessagePersistenceOptions _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMachineInstanceInfo _machineInstanceInfo;

    public MessagePersistenceBackgroundService(
        ILogger<MessagePersistenceBackgroundService> logger,
        IOptions<MessagePersistenceOptions> options,
        IServiceScopeFactory serviceScopeFactory,
        IMachineInstanceInfo machineInstanceInfo)
    {
        _logger = logger;
        _options = options.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _machineInstanceInfo = machineInstanceInfo;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            $"MessagePersistence Background Service is starting on client '{_machineInstanceInfo.ClientId}' and group '{_machineInstanceInfo.ClientGroup}'.");

        await ProcessAsync(cancellationToken);
    }

    private async Task ProcessAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<IMessagePersistenceService>();
                await service.ProcessAllAsync(stoppingToken);
            }

            var delay = _options.Interval is { }
                ? TimeSpan.FromSeconds((int)_options.Interval)
                : TimeSpan.FromSeconds(30);

            await Task.Delay(delay, stoppingToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}