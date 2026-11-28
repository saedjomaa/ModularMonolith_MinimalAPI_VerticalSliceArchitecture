namespace CourseHub.Users.Infrastructure.Seeding;

/// <summary>
/// The development instructor account (USR-04). Read from "Users:SeedInstructor"
/// in appsettings.Development.json. These are local test values only.
/// </summary>
internal sealed class SeedInstructorOptions
{
    public const string SectionName = "Users:SeedInstructor";

    public string? Email { get; init; }

    public string? Password { get; init; }

    public string FullName { get; init; } = "Default Instructor";

    public string PhoneNumber { get; init; } = "0000000000";
}
