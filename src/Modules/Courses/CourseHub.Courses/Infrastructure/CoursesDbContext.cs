using CourseHub.Courses.Domain;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Courses.Infrastructure;

/// <summary>The Courses module's own DbContext, in the "courses" schema.</summary>
internal sealed class CoursesDbContext(DbContextOptions<CoursesDbContext> options) : DbContext(options)
{
    public const string Schema = "courses";

    public DbSet<Course> Courses => Set<Course>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoursesDbContext).Assembly);
    }
}
