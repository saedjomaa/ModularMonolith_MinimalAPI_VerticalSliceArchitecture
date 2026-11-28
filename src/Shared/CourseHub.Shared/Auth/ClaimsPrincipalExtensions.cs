using System.Security.Claims;

namespace CourseHub.Shared.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Id of the signed-in user. Call it only on endpoints that require authorization.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal principal) =>
        principal.TryGetUserId()
        ?? throw new InvalidOperationException("The current principal has no valid user id claim.");

    /// <summary>
    /// Id of the signed-in user, or <c>null</c> for anonymous requests.
    /// Use it on endpoints that allow anonymous access.
    /// </summary>
    public static Guid? TryGetUserId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
}
