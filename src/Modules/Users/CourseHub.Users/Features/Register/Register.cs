using System.ComponentModel.DataAnnotations;
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

namespace CourseHub.Users.Features.Register;

/// <summary>Creates a new account. Every new account is a Student (USR-01, USR-02, USR-03, USR-05).</summary>
internal static class Register
{
    internal sealed record Request(
        [Required, EmailAddress, MaxLength(256)] string Email,
        [Required, MinLength(8)] string Password,
        [Required, MaxLength(ApplicationUser.FullNameMaxLength)] string FullName,
        [Required, Phone, MaxLength(ApplicationUser.PhoneNumberMaxLength)] string PhoneNumber);

    internal sealed record Response(Guid Id, string Email);

    internal sealed class Handler(
        UserManager<ApplicationUser> userManager,
        UsersDbContext db,
        TimeProvider clock) : IHandler
    {
        public async Task<Result<Response>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var created = ApplicationUser.Create(request.Email, request.FullName, request.PhoneNumber, clock.GetUtcNow());
            if (created.IsFailure)
            {
                return created.Error;
            }

            var user = created.Value;

            // Creating the user and assigning the role succeed or fail together.
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            var createResult = await userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return UserErrors.FromIdentity(createResult); // USR-02 (duplicate email) or a weak password
            }

            var roleResult = await userManager.AddToRoleAsync(user, Roles.Student); // USR-03
            if (!roleResult.Succeeded)
            {
                return UserErrors.FromIdentity(roleResult);
            }

            await transaction.CommitAsync(cancellationToken);
            return new Response(user.Id, user.Email!);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapPost("/register", async Task<Results<Created<Response>, ProblemHttpResult>> (
                    Request request, Handler handler, CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(request, cancellationToken);
                    return result.IsSuccess
                        ? TypedResults.Created("/users/me", result.Value)
                        : result.ToProblem();
                })
                .AllowAnonymous()
                .ProducesErrors(UserErrors.InvalidFullName, UserErrors.InvalidPhoneNumber, UserErrors.IdentityValidation, UserErrors.EmailTaken)
                .WithName("Register")
                .WithSummary("Register a new student account");
    }
}
