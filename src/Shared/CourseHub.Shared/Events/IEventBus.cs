namespace CourseHub.Shared.Events;

public interface IEventBus
{
    /*
     آلية التواصل غير المباشر بين الوحدات: وحدة تعلن أن شيئًا حدث، ووحدات أخرى تتصرف بناءً عليه، بدون أن تعرف أي منها الأخرى.
     
    الوحدة الناشرة تستخدم هذه الواجهة فقط، ولا تعرف من سيستمع.
    القاعدة: انشر الحدث بعد نجاح
    SaveChanges، 
    حتى لا تُعلن عن شيء لم يُحفظ فعلًا.
     */
    /// <summary>
    /// Publishes an integration event to every handler in every module.
    /// Call it only after the publisher's changes are saved.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}
