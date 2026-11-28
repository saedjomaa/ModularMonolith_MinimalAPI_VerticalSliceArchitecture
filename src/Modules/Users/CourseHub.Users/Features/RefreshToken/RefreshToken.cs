using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CourseHub.Shared.Endpoints;
using CourseHub.Shared.Handlers;
using CourseHub.Shared.Results;
using CourseHub.Users.Domain;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace CourseHub.Users.Features.RefreshToken;

/// <summary>
/// Exchanges a valid refresh token for new tokens. The new token carries the user's
/// current roles, so a promoted user (USR-06) gets the Instructor role after a refresh.
/// </summary>
internal static class RefreshToken
{
    internal sealed record Request([Required] string RefreshToken);

    internal sealed class Handler(
        SignInManager<ApplicationUser> signInManager,
        IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
        TimeProvider clock) : IHandler
    {
        public async Task<Result<ClaimsPrincipal>> HandleAsync(Request request)
        {
            var protector = bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
            var ticket = protector.Unprotect(request.RefreshToken);

            if (ticket?.Properties.ExpiresUtc is not { } expiresAt || clock.GetUtcNow() >= expiresAt)
            {
                return UserErrors.InvalidRefreshToken;
            }

            // Fails when the security stamp changed (e.g. password change), which revokes old refresh tokens.
            if (await signInManager.ValidateSecurityStampAsync(ticket.Principal) is not { } user)
            {
                return UserErrors.InvalidRefreshToken;
            }

            return await signInManager.CreateUserPrincipalAsync(user);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapPost("/refresh", async Task<Results<SignInHttpResult, ProblemHttpResult>> (
                    Request request, Handler handler) =>
                {
                    var result = await handler.HandleAsync(request);
                    return result.IsSuccess
                        ? TypedResults.SignIn(result.Value, authenticationScheme: IdentityConstants.BearerScheme)
                        : result.ToProblem();
                })
                .AllowAnonymous()
                .Produces<AccessTokenResponse>()
                .ProducesErrors(UserErrors.InvalidRefreshToken)
                .WithName("RefreshToken")
                .WithSummary("Get new tokens using a refresh token");
    }
}
