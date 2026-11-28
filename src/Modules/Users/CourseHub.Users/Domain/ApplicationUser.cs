using CourseHub.Shared.Results;
using Microsoft.AspNetCore.Identity;

namespace CourseHub.Users.Domain;

/// <summary>
/// A registered user (student or instructor). The email is the login name (USR-01).
/// Identity owns the password, roles and security stamp; this class owns the profile (USR-05).
/// </summary>
internal sealed class ApplicationUser : IdentityUser<Guid>
{
    public const int FullNameMaxLength = 200;
    public const int PhoneNumberMaxLength = 30;

    /// <summary>Required by EF Core.</summary>
    private ApplicationUser()
    {
    }

    public string FullName { get; private set; } = string.Empty;

    public DateTimeOffset RegisteredAt { get; private set; }

    public static Result<ApplicationUser> Create(string email, string fullName, string phoneNumber, DateTimeOffset now)
    {
        var validation = ValidateProfile(fullName, phoneNumber);
        if (validation.IsFailure)
        {
            return validation.Error;
        }

        return new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = email.Trim(),
            Email = email.Trim(),
            FullName = fullName.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            RegisteredAt = now
        };
    }

    // USR-05
    public Result UpdateProfile(string fullName, string phoneNumber)
    {
        var validation = ValidateProfile(fullName, phoneNumber);
        if (validation.IsFailure)
        {
            return validation;
        }

        FullName = fullName.Trim();
        PhoneNumber = phoneNumber.Trim();
        return Result.Success();
    }

    private static Result ValidateProfile(string fullName, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length > FullNameMaxLength)
        {
            return UserErrors.InvalidFullName;
        }

        if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Trim().Length > PhoneNumberMaxLength)
        {
            return UserErrors.InvalidPhoneNumber;
        }

        return Result.Success();
    }
}
