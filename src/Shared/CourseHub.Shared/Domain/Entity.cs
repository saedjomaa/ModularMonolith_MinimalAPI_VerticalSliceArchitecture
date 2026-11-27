namespace CourseHub.Shared.Domain;

/*
     هذا هو الكلاس الأساسي لأي كيان. الكيان شيء له هوية ثابتة (Id)، مثل كورس أو فاتورة أو طالب.
    ما يقدمه:
    الخاصية Id: من نوع عام TId، فيمكن أن تكون Guid أو غيره. وهي protected init، أي تُحدَّد مرة واحدة عند الإنشاء، ولا يمكن تغييرها من خارج الكيان.
    مُنشئان (Constructors):
    Entity(TId id): للإنشاء من الكود.
    Entity() بدون معاملات: يحتاجه EF Core عند قراءة الكيان من قاعدة البيانات.
*/
/// <summary>
/// Base class for entities. Two entities are equal when they have the same type and id.
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity(TId id) => Id = id;

    /// <summary>Required by EF Core.</summary>
    protected Entity()
    {
        Id = default!;
    }

    public TId Id { get; protected init; }

    public bool Equals(Entity<TId>? other) =>
        other is not null && other.GetType() == GetType() && EqualityComparer<TId>.Default.Equals(Id, other.Id);

    public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
