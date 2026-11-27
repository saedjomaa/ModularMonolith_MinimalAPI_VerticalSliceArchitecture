namespace CourseHub.Shared.Domain;

/*
    هذا الكلاس يرث من Entity، ويمثل جذر التجميع (Aggregate Root): الكيان الرئيسي الذي يحرس مجموعة من البيانات المرتبطة، ولا يُعدَّل شيء منها إلا من خلاله.
   مثال من مشروعنا: الـ Aggregate المسمى CourseSeats في وحدة التسجيلات يضم التسجيلات وقائمة الانتظار. لا يمكن إضافة تسجيل مباشرة، بل عبر دالة مثل courseSeats.Enroll(studentId)، وهي التي تتحقق من السعة (ENR-02).
   ما يضيفه فوق Entity:
   قائمة Domain Events: عندما تحدث عملية مهمة داخل الكيان، يسجّل حدثًا عبر Raise(...).
   الخاصية DomainEvents تتيح قراءة القائمة فقط، فلا يستطيع أحد تعديلها من الخارج.
   الدالة ClearDomainEvents() تفرّغ القائمة بعد معالجة الأحداث، حتى لا تُعالج مرتين.
   */
/// <summary>
/// Entry point of a consistency boundary. Collects domain events raised by its methods;
/// the module turns them into side effects (or integration events) after saving.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    /// <summary>Required by EF Core.</summary>
    protected AggregateRoot()
    {
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
