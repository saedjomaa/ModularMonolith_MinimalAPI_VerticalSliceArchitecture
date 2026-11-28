using CourseHub.Shared.Auth;
using CourseHub.Shared.Endpoints;
using CourseHub.Shared.Handlers;
using CourseHub.Shared.Results;
using CourseHub.Users.Domain;
using CourseHub.Users.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace CourseHub.Users.Features.PromoteToInstructor;

/// <summary>
/// An existing instructor promotes a user to Instructor (USR-06). The Student role is replaced.
/// The promoted user gets the new role in their next token (login or refresh).
/// </summary>
internal static class PromoteToInstructor
{
    internal sealed class Handler(UserManager<ApplicationUser> userManager, UsersDbContext db) : IHandler
    {
        public async Task<Result> HandleAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return UserErrors.NotFound;
            }

            if (await userManager.IsInRoleAsync(user, Roles.Instructor))
            {
                return UserErrors.AlreadyInstructor;
            }

            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            var addResult = await userManager.AddToRoleAsync(user, Roles.Instructor);
            if (!addResult.Succeeded)
            {
                return UserErrors.FromIdentity(addResult);
            }

            if (await userManager.IsInRoleAsync(user, Roles.Student))
            {
                var removeResult = await userManager.RemoveFromRoleAsync(user, Roles.Student);
                if (!removeResult.Succeeded)
                {
                    return UserErrors.FromIdentity(removeResult);
                }
            }

            await transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapPost("/{userId:guid}/promote", async Task<Results<NoContent, ProblemHttpResult>> (
                    Guid userId, Handler handler, CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(userId, cancellationToken);
                    return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
                })
                .RequireAuthorization(Policies.InstructorOnly)
                .ProducesErrors(UserErrors.NotFound, UserErrors.AlreadyInstructor, UserErrors.IdentityValidation)
                .WithName("PromoteToInstructor")
                .WithSummary("Promote a user to instructor (instructors only)");
    }
}
