namespace CourseHub.Shared.Domain;

/*
  هذه واجهة فارغة (Marker Interface) تُعلَّم بها أحداث الـ Domain، أي الأشياء التي حدثت داخل وحدة واحدة.
 */
/// <summary>
/// Something that happened inside one module's domain. Never leaves the module;
/// use an <see cref="Events.IIntegrationEvent"/> to notify other modules.
/// </summary>
public interface IDomainEvent;
