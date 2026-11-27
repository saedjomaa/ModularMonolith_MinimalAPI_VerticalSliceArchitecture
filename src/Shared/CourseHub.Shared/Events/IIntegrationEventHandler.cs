namespace CourseHub.Shared.Events;


/*
 الوحدة التي تريد التصرف عند حدث معيّن تكتب كلاسًا يطبّق هذه الواجهة، وتضعه في مجلد IntegrationEventHandlers داخلها
 */
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken);
}
