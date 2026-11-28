using CourseHub.Shared.Domain;
using CourseHub.Shared.Results;

namespace CourseHub.Courses.Domain;

/*
 الكورس هو الـ Aggregate الوحيد في وحدة Courses، وكل قواعده هنا:
 - CRS-01: العنوان والوصف والمدرّب والسعر والحالة والسعة.
 - CRS-02: يبدأ كمسودة (Draft).
 - CRS-03: لا يُنشر بدون سعر ومدرّب، ولا يُحذف سعر كورس منشور.
 - CRS-04: السعة عدد صحيح موجب.
 - CRS-05: المدرّب المالك فقط يعدّل أو ينشر.
 - CRS-06: الكورس المسودة لا يراه إلا مالكه.
 */
/// <summary>A course offered by an instructor. Owns rules CRS-01 … CRS-06.</summary>
internal sealed class Course : AggregateRoot<Guid>
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int MaxCapacity = 10_000;
    public const double MinPrice = 0.01;
    public const double MaxPrice = 1_000_000;

    /// <summary>Required by EF Core.</summary>
    private Course()
    {
    }

    private Course(Guid id, Guid instructorId, DateTimeOffset createdAt)
        : base(id)
    {
        InstructorId = instructorId;
        Status = CourseStatus.Draft; // CRS-02
        CreatedAt = createdAt;
    }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    /// <summary>The owning instructor (a user id from the Users module).</summary>
    public Guid InstructorId { get; private set; }

    /// <summary>Null while the price is not set yet (allowed only for drafts, CRS-03).</summary>
    public decimal? Price { get; private set; }

    public int Capacity { get; private set; }

    public CourseStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    /// <summary>Optimistic concurrency token (SQL Server rowversion).</summary>
    public byte[] RowVersion { get; private set; } = [];

    public bool IsPublished => Status == CourseStatus.Published;

    public static Result<Course> Create(
        Guid instructorId, string title, string? description, decimal? price, int capacity, DateTimeOffset now)
    {
        if (instructorId == Guid.Empty)
        {
            return CourseErrors.InstructorRequired;
        }

        var course = new Course(Guid.CreateVersion7(), instructorId, now);
        var details = course.ApplyDetails(title, description, price, capacity);
        return details.IsSuccess ? course : details.Error;
    }

    /// <summary>Changes title, description, price and capacity (CRS-05: owner only).</summary>
    public Result Update(Guid actorId, string title, string? description, decimal? price, int capacity)
    {
        var owner = EnsureOwnedBy(actorId);
        if (owner.IsFailure)
        {
            return owner;
        }

        // CRS-03: a published course must keep its price.
        if (IsPublished && price is null)
        {
            return CourseErrors.PriceRequired;
        }

        return ApplyDetails(title, description, price, capacity);
    }

    /// <summary>Publishes the course (CRS-03: needs a price and an instructor; CRS-05: owner only).</summary>
    public Result Publish(Guid actorId, DateTimeOffset now)
    {
        var owner = EnsureOwnedBy(actorId);
        if (owner.IsFailure)
        {
            return owner;
        }

        if (IsPublished)
        {
            return CourseErrors.AlreadyPublished;
        }

        if (InstructorId == Guid.Empty)
        {
            return CourseErrors.InstructorRequired; // CRS-03
        }

        if (Price is null)
        {
            return CourseErrors.PriceRequired; // CRS-03
        }

        Status = CourseStatus.Published;
        PublishedAt = now;
        return Result.Success();
    }

    /// <summary>CRS-06: published courses are visible to everyone; drafts only to their owner.</summary>
    public bool IsVisibleTo(Guid? userId) => IsPublished || (userId is { } id && id == InstructorId);

    // CRS-05
    private Result EnsureOwnedBy(Guid actorId) =>
        actorId == InstructorId ? Result.Success() : CourseErrors.NotOwner;

    private Result ApplyDetails(string title, string? description, decimal? price, int capacity)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > TitleMaxLength)
        {
            return CourseErrors.InvalidTitle;
        }

        if (description is { Length: > DescriptionMaxLength })
        {
            return CourseErrors.InvalidDescription;
        }

        if (price is { } p && (p < (decimal)MinPrice || p > (decimal)MaxPrice))
        {
            return CourseErrors.InvalidPrice;
        }

        if (capacity is < 1 or > MaxCapacity)
        {
            return CourseErrors.InvalidCapacity; // CRS-04
        }

        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Price = price is { } value ? decimal.Round(value, 2) : null;
        Capacity = capacity;
        return Result.Success();
    }
}
