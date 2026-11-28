using CourseHub.Courses.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourseHub.Courses.Infrastructure.Configurations;

internal sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Title).HasMaxLength(Course.TitleMaxLength).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(Course.DescriptionMaxLength);
        builder.Property(c => c.Price).HasPrecision(18, 2);
        builder.Property(c => c.Capacity).IsRequired();
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.RowVersion).IsRowVersion();

        // Only the user id from the Users module; no foreign key across modules.
        builder.Property(c => c.InstructorId).IsRequired();
        builder.HasIndex(c => c.InstructorId);
        builder.HasIndex(c => c.Status);

        builder.Ignore(c => c.IsPublished);
        builder.Ignore(c => c.DomainEvents);
    }
}
