namespace CourseHub.Users.Contracts;

/// <summary>
/// What other modules may ask the Users module. Used later by Enrollment
/// to show the students enrolled in an instructor's course (ENR-07).
/// </summary>
public interface IUsersApi
{
    Task<UserSummary?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserSummary>> GetUsersAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);
}

public sealed record UserSummary(Guid Id, string Email, string FullName, string? PhoneNumber);
