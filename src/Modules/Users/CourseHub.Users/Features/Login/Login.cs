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

namespace CourseHub.Users.Features.Login;

/*
 تسجيل الدخول على خطوتين واضحتين:
 1) الـ Handler يتحقق من البريد وكلمة المرور والقفل (Lockout) فقط، ويرجع الـ ClaimsPrincipal. لا يكتب شيئًا في الاستجابة.
 2) الـ Endpoint يرجع TypedResults.SignIn(...)، وعند تنفيذه يكتب BearerTokenHandler
    (داخل ASP.NET Core) استجابة AccessTokenResponse: accessToken و refreshToken و expiresIn.
 نفس الأسلوب المستخدم في RefreshToken.
 */
/// <summary>
/// Signs in with email and password (USR-01). The handler only validates the credentials;
/// the endpoint returns <see cref="TypedResults.SignIn"/>, whose execution makes the Identity
/// bearer-token handler write the <see cref="AccessTokenResponse"/>.
/// </summary>
internal static class Login
{
    internal sealed record Request(
        [Required, EmailAddress] string Email,
        [Required] string Password);

    internal sealed class Handler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager) : IHandler
    {
        public async Task<Result<ClaimsPrincipal>> HandleAsync(Request request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return UserErrors.InvalidCredentials;
            }

            // Checks the password and lockout without signing in (no response is written here).
            var check = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (check.IsLockedOut)
            {
                return UserErrors.LockedOut;
            }

            if (!check.Succeeded)
            {
                return UserErrors.InvalidCredentials;
            }

            return await signInManager.CreateUserPrincipalAsync(user);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapPost("/login", async Task<Results<SignInHttpResult, ProblemHttpResult>> (
                    Request request, Handler handler) =>
                {
                    var result = await handler.HandleAsync(request);
                    return result.IsSuccess
                        ? TypedResults.SignIn(result.Value, authenticationScheme: IdentityConstants.BearerScheme)
                        : result.ToProblem();
                })
                .AllowAnonymous()
                .Produces<AccessTokenResponse>()
                .ProducesErrors(UserErrors.InvalidCredentials, UserErrors.LockedOut)
                .WithName("Login")
                .WithSummary("Sign in and receive an access token and a refresh token");
    }
}
