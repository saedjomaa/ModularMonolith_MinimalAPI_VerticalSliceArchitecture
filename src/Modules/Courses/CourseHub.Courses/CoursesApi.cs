using CourseHub.Courses.Contracts;
using CourseHub.Courses.Domain;
using CourseHub.Courses.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Courses;

/// <summary>Implementation of the module's public query contract.</summary>
internal sealed class CoursesApi(CoursesDbContext db) : ICoursesApi
{
    public Task<CourseSummary?> GetCourseAsync(Guid courseId, CancellationToken cancellationToken = default) =>
        db.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new CourseSummary(c.Id, c.Title, c.InstructorId, c.Price, c.Capacity, c.Status == CourseStatus.Published))
            .FirstOrDefaultAsync(cancellationToken);
}
