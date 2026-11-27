namespace CourseHub.Shared.Events;

/*
 كل حدث بين الوحدات يطبّق هذه الواجهة. لكل حدث رقم فريد (EventId) يفيد في السجلات (Logs) وفي منع المعالجة المكررة لاحقًا، ووقت الحدوث (OccurredAt).
الأحداث الفعلية تُكتب كـ record في مشروع Contracts للوحدة التي تنشرها. مثال سيُكتب في المرحلة 4:
public sealed record PaymentSucceeded(
    Guid EventId, DateTimeOffset OccurredAt,
    Guid EnrollmentId, Guid InvoiceId) : IIntegrationEvent;
 */
/// <summary>
/// An event published by one module for other modules. Declared as a record
/// in the publishing module's <c>Contracts</c> project.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAt { get; }
}
