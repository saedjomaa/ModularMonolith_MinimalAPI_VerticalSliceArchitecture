namespace CourseHub.Shared.Events;

public interface IEventBus
{
    /// <summary>
    /// Publishes an integration event to every handler in every module.
    /// Call it only after the publisher's changes are saved.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}
