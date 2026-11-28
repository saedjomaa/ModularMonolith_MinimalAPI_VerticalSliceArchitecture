namespace CourseHub.Shared.Auth;

/// <summary>
/// Authorization policy names used by endpoints in every module.
/// The policies themselves are registered by the Users module.
/// </summary>
public static class Policies
{
    public const string StudentOnly = "StudentOnly";

    public const string InstructorOnly = "InstructorOnly";
}
