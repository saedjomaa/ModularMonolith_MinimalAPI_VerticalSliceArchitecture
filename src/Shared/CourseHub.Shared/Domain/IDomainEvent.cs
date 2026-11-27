namespace CourseHub.Shared.Domain;

/// <summary>
/// Something that happened inside one module's domain. Never leaves the module;
/// use an <see cref="Events.IIntegrationEvent"/> to notify other modules.
/// </summary>
public interface IDomainEvent;
