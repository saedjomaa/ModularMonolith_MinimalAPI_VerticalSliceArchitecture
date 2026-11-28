using System.Security.Claims;
using CourseHub.Shared.Auth;
using CourseHub.Shared.Endpoints;
using CourseHub.Shared.Handlers;
using CourseHub.Shared.Results;
using CourseHub.Users.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace CourseHub.Users.Features.GetMyProfile;

/// <summary>Returns the signed-in user's profile and roles (USR-05).</summary>
internal static class GetMyProfile
{
    internal sealed record Response(
        Guid Id,
        string Email,
        string FullName,
        string? PhoneNumber,
        DateTimeOffset RegisteredAt,
        IReadOnlyList<string> Roles);

    internal sealed class Handler(UserManager<ApplicationUser> userManager) : IHandler
    {
        public async Task<Result<Response>> HandleAsync(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return UserErrors.NotFound;
            }

            var roles = await userManager.GetRolesAsync(user);

            return new Response(user.Id, user.Email!, user.FullName, user.PhoneNumber, user.RegisteredAt, [.. roles]);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapGet("/me", async Task<Results<Ok<Response>, ProblemHttpResult>> (
                    ClaimsPrincipal principal, Handler handler) =>
                {
                    var result = await handler.HandleAsync(principal.GetUserId());
                    return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
                })
                .RequireAuthorization()
                .ProducesErrors(UserErrors.NotFound)
                .WithName("GetMyProfile")
                .WithSummary("Get the signed-in user's profile");
    }
}
