using System.ComponentModel.DataAnnotations;
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

namespace CourseHub.Users.Features.UpdateMyProfile;

/// <summary>Updates the signed-in user's full name and phone number (USR-05).</summary>
internal static class UpdateMyProfile
{
    internal sealed record Request(
        [Required, MaxLength(ApplicationUser.FullNameMaxLength)] string FullName,
        [Required, Phone, MaxLength(ApplicationUser.PhoneNumberMaxLength)] string PhoneNumber);

    internal sealed class Handler(UserManager<ApplicationUser> userManager) : IHandler
    {
        public async Task<Result> HandleAsync(Guid userId, Request request)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return UserErrors.NotFound;
            }

            var updated = user.UpdateProfile(request.FullName, request.PhoneNumber);
            if (updated.IsFailure)
            {
                return updated;
            }

            var saveResult = await userManager.UpdateAsync(user);
            return saveResult.Succeeded ? Result.Success() : UserErrors.FromIdentity(saveResult);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapPut("/me", async Task<Results<NoContent, ProblemHttpResult>> (
                    Request request, ClaimsPrincipal principal, Handler handler) =>
                {
                    var result = await handler.HandleAsync(principal.GetUserId(), request);
                    return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
                })
                .RequireAuthorization()
                .ProducesErrors(UserErrors.InvalidFullName, UserErrors.InvalidPhoneNumber, UserErrors.IdentityValidation, UserErrors.NotFound)
                .WithName("UpdateMyProfile")
                .WithSummary("Update the signed-in user's full name and phone number");
    }
}
