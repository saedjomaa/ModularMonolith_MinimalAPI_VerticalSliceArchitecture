using System.Reflection;
using CourseHub.Billing;
using CourseHub.Courses;
using CourseHub.Enrollment;
using CourseHub.Notifications;
using CourseHub.Shared;
using CourseHub.Users;

namespace CourseHub.ArchitectureTests;

/// <summary>Every module of the solution, with its main and Contracts assemblies.</summary>
public static class Modules
{
    public const string Prefix = "CourseHub.";

    public static readonly Assembly Shared = typeof(SharedServiceExtensions).Assembly;

    public static readonly IReadOnlyDictionary<string, Assembly> Main = new Dictionary<string, Assembly>
    {
        ["Users"] = typeof(UsersModule).Assembly,
        ["Courses"] = typeof(CoursesModule).Assembly,
        ["Enrollment"] = typeof(EnrollmentModule).Assembly,
        ["Billing"] = typeof(BillingModule).Assembly,
        ["Notifications"] = typeof(NotificationsModule).Assembly,
    };

    public static TheoryData<string> Names => [.. Main.Keys];

    public static Assembly Contracts(string module) => Assembly.Load($"{Prefix}{module}.Contracts");

    /// <summary>
    /// Solution assemblies that <paramref name="assembly"/> actually uses
    /// (the compiler drops unused project references).
    /// </summary>
    public static IEnumerable<string> SolutionReferences(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name!)
            .Where(name => name.StartsWith(Prefix, StringComparison.Ordinal));
}
