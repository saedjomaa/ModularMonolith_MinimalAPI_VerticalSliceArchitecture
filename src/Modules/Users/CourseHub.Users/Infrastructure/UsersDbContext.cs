using CourseHub.Users.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Users.Infrastructure;

/// <summary>
/// The Users module's own DbContext. It holds the ASP.NET Core Identity tables in the "users" schema.
/// </summary>
internal sealed class UsersDbContext(DbContextOptions<UsersDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public const string Schema = "users";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
    }
}
