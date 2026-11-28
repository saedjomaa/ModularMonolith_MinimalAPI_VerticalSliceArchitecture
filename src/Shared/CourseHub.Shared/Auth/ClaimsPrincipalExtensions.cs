using System.Security.Claims;

namespace CourseHub.Shared.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Id of the signed-in user. Call it only on endpoints that require authorization.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new InvalidOperationException("The current principal has no valid user id claim.");
    }
}
