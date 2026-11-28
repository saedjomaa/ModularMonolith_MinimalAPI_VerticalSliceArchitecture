using CourseHub.Users.Contracts;
using CourseHub.Users.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Users;

/// <summary>Implementation of the module's public query contract.</summary>
internal sealed class UsersApi(UsersDbContext db) : IUsersApi
{
    public Task<UserSummary?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserSummary(u.Id, u.Email!, u.FullName, u.PhoneNumber))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<UserSummary>> GetUsersAsync(
        IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default) =>
        await db.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new UserSummary(u.Id, u.Email!, u.FullName, u.PhoneNumber))
            .ToListAsync(cancellationToken);
}
