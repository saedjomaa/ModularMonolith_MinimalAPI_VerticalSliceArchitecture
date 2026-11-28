using CourseHub.Shared.Results;
using Microsoft.AspNetCore.Identity;

namespace CourseHub.Users.Domain;

internal static class UserErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Users.NotFound", "The user was not found.");

    public static readonly Error EmailTaken =
        Error.Conflict("Users.EmailTaken", "An account with this email already exists."); // USR-02

    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Users.InvalidCredentials", "The email or password is incorrect.");

    public static readonly Error LockedOut =
        Error.Unauthorized("Users.LockedOut", "The account is temporarily locked after too many failed attempts.");

    public static readonly Error InvalidRefreshToken =
        Error.Unauthorized("Users.InvalidRefreshToken", "The refresh token is invalid, expired or revoked.");

    public static readonly Error InvalidFullName =
        Error.Validation("Users.InvalidFullName", $"Full name is required and must be at most {ApplicationUser.FullNameMaxLength} characters.", "fullName");

    public static readonly Error InvalidPhoneNumber =
        Error.Validation("Users.InvalidPhoneNumber", $"Phone number is required and must be at most {ApplicationUser.PhoneNumberMaxLength} characters.", "phoneNumber");

    /// <summary>Representative error for documentation; <see cref="FromIdentity"/> returns it with the actual field errors.</summary>
    public static readonly Error IdentityValidation =
        Error.Validation("Users.IdentityValidation", "The email or password does not meet the requirements (e.g. password strength). See 'errors'.");

    public static readonly Error AlreadyInstructor =
        Error.Conflict("Users.AlreadyInstructor", "The user is already an instructor."); // USR-06

    /// <summary>
    /// Maps a failed <see cref="IdentityResult"/> to an <see cref="Error"/>: a duplicate email becomes
    /// <see cref="EmailTaken"/> (409); other errors become field errors under <c>password</c> or <c>email</c> (400).
    /// </summary>
    public static Error FromIdentity(IdentityResult result)
    {
        var codes = result.Errors.Select(e => e.Code).ToHashSet();
        if (codes.Contains(nameof(IdentityErrorDescriber.DuplicateEmail)) || codes.Contains(nameof(IdentityErrorDescriber.DuplicateUserName)))
        {
            return EmailTaken;
        }

        var fieldErrors = result.Errors
            .GroupBy(e => FieldFor(e.Code))
            .ToDictionary(group => group.Key, group => group.Select(e => e.Description).ToArray());

        return Error.Validation(IdentityValidation.Code, IdentityValidation.Description, fieldErrors);
    }

    private static string FieldFor(string identityErrorCode) => identityErrorCode switch
    {
        _ when identityErrorCode.StartsWith("Password", StringComparison.Ordinal) => "password",
        _ when identityErrorCode.Contains("Email", StringComparison.Ordinal) => "email",
        _ when identityErrorCode.Contains("UserName", StringComparison.Ordinal) => "email",
        _ => string.Empty
    };
}
