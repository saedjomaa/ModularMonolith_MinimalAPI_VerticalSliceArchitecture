using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CourseHub.Shared.Events;

/*
 عند استدعاء PublishAsync يحدث التالي:
كل عملية نشر تنشئ Scope واحدًا جديدًا تشترك فيه كل الـ Handlers لهذا الحدث， 
فتحصل على DbContext منفصل عن DbContext الناشر. 
طبعا لان كل مشروع له DbContext منفصل، فلا مشكلة في أن يكون لكل Handler DbContext مختلف عن الناشر، ولا عن Handlers الآخرين.
يجلب كل الـ Handlers المسجلة لهذا النوع من الأحداث، من كل الوحدات.
يستدعيها واحدًا تلو الآخر، ويسجّل سطرًا في الـ Log لكل منها.

يُسجَّل كـ Singleton في AddSharedServices().

الدفع نجح --> التسجيل نجح --> إشعار بالنجاح

Billing:      الدفع نجح → SaveChanges → eventBus.PublishAsync(PaymentSucceeded)
                                               │
Enrollment:   PaymentSucceededHandler ◄────────┘
              تأكيد التسجيل → SaveChanges → PublishAsync(EnrollmentConfirmed)
                                               │
Notifications: EnrollmentConfirmedHandler ◄────┘
               إرسال رسالة + حفظها في سجل الرسائل


لا Billing تعرف Enrollment، ولا Enrollment تعرف Notifications. كل واحدة تعرف فقط Contracts الوحدة التي تستمع لأحداثها.
 */
/// <summary>
/// In-process event bus. Each publish creates one new DI scope, shared by all handlers
/// of that event, so handlers get DbContexts separate from the publisher's.
/// Known limitation (see docs/Architecture.md §4): no outbox, so an event
/// is lost if the process stops between saving and publishing.
/// </summary>
internal sealed partial class InMemoryEventBus(
    IServiceScopeFactory scopeFactory,
    ILogger<InMemoryEventBus> logger) : IEventBus
{
    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        //ينشئ Scope واحدًا لكل عملية نشر تشترك فيه كل الـ Handlers.
        await using var scope = scopeFactory.CreateAsyncScope();

        //يجلب كل الـ Handlers المسجلة لهذا النوع من الأحداث، من كل الوحدات.
        var handlers = scope.ServiceProvider.GetServices<IIntegrationEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            //يستدعيها واحدًا تلو الآخر، ويسجّل سطرًا في الـ Log لكل منها.
            LogHandling(typeof(TEvent).Name, integrationEvent.EventId, handler.GetType().FullName);
            await handler.HandleAsync(integrationEvent, cancellationToken);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling {EventName} {EventId} with {Handler}")]
    private partial void LogHandling(string eventName, Guid eventId, string? handler);
}
