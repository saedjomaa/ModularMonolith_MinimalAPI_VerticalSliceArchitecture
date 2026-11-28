using CourseHub.Users.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CourseHub.Users.Infrastructure.Configurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FullName)
            .HasMaxLength(ApplicationUser.FullNameMaxLength)
            .IsRequired();

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(ApplicationUser.PhoneNumberMaxLength);

        builder.Property(u => u.RegisteredAt)
            .IsRequired();
    }
}
