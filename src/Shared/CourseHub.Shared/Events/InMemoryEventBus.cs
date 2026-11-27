using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CourseHub.Shared.Events;

/// <summary>
/// In-process event bus. Each handler runs in its own DI scope so it gets its own
/// DbContext. Known limitation (see docs/Architecture.md §4): no outbox, so an event
/// is lost if the process stops between saving and publishing.
/// </summary>
internal sealed partial class InMemoryEventBus(
    IServiceScopeFactory scopeFactory,
    ILogger<InMemoryEventBus> logger) : IEventBus
{
    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var handlers = scope.ServiceProvider.GetServices<IIntegrationEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            LogHandling(typeof(TEvent).Name, integrationEvent.EventId, handler.GetType().FullName);
            await handler.HandleAsync(integrationEvent, cancellationToken);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {EventName} {EventId} with {Handler}")]
    private partial void LogHandling(string eventName, Guid eventId, string? handler);
}
