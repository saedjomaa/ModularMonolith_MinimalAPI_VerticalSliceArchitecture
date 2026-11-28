using CourseHub.Shared.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourseHub.Users.Infrastructure.Configurations;

/// <summary>
/// Seeds the two roles (USR-03) through the migration, so they exist in every environment.
/// Ids and concurrency stamps are fixed so the migration stays stable.
/// </summary>
internal sealed class RoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    public static readonly Guid StudentRoleId = new("0197a1c0-0000-7000-8000-000000000001");
    public static readonly Guid InstructorRoleId = new("0197a1c0-0000-7000-8000-000000000002");

    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        builder.HasData(
            new IdentityRole<Guid>
            {
                Id = StudentRoleId,
                Name = Roles.Student,
                NormalizedName = Roles.Student.ToUpperInvariant(),
                ConcurrencyStamp = "student-role"
            },
            new IdentityRole<Guid>
            {
                Id = InstructorRoleId,
                Name = Roles.Instructor,
                NormalizedName = Roles.Instructor.ToUpperInvariant(),
                ConcurrencyStamp = "instructor-role"
            });
    }
}
