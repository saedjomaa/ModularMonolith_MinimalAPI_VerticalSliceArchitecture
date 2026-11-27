namespace CourseHub.Shared.Events;

/// <summary>
/// An event published by one module for other modules. Declared as a record
/// in the publishing module's <c>Contracts</c> project.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAt { get; }
}
