namespace CourseHub.Courses.Contracts;

/// <summary>
/// What other modules may ask the Courses module. Enrollment uses it to check that a course
/// is published (ENR-06), to read its capacity (ENR-02) and owner (ENR-07); Billing uses the price (BIL-01).
/// </summary>
public interface ICoursesApi
{
    Task<CourseSummary?> GetCourseAsync(Guid courseId, CancellationToken cancellationToken = default);
}

public sealed record CourseSummary(
    Guid Id,
    string Title,
    Guid InstructorId,
    decimal? Price,
    int Capacity,
    bool IsPublished);
