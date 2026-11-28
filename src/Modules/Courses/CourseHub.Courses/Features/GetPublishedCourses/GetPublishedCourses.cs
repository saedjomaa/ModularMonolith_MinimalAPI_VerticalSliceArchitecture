using CourseHub.Courses.Domain;
using CourseHub.Courses.Infrastructure;
using CourseHub.Shared.Endpoints;
using CourseHub.Shared.Handlers;
using CourseHub.Users.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Courses.Features.GetPublishedCourses;

/// <summary>
/// The public catalog: published courses only (CRS-06), newest first, paged.
/// Instructor names come from the Users module through <see cref="IUsersApi"/>.
/// </summary>
internal static class GetPublishedCourses
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    internal sealed record Item(
        Guid Id,
        string Title,
        string? Description,
        decimal Price,
        int Capacity,
        Guid InstructorId,
        string? InstructorName,
        DateTimeOffset PublishedAt);

    internal sealed record Response(IReadOnlyList<Item> Items, int Page, int PageSize, int TotalCount);

    internal sealed class Handler(CoursesDbContext db, IUsersApi usersApi) : IHandler
    {
        public async Task<Response> HandleAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            var query = db.Courses.AsNoTracking().Where(c => c.Status == CourseStatus.Published);

            var totalCount = await query.CountAsync(cancellationToken);
            var courses = await query
                .OrderByDescending(c => c.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var instructors = await usersApi.GetUsersAsync([.. courses.Select(c => c.InstructorId).Distinct()], cancellationToken);
            var names = instructors.ToDictionary(u => u.Id, u => u.FullName);

            var items = courses
                .Select(c => new Item(
                    c.Id, c.Title, c.Description, c.Price!.Value, c.Capacity,
                    c.InstructorId, names.GetValueOrDefault(c.InstructorId), c.PublishedAt!.Value))
                .ToList();

            return new Response(items, page, pageSize, totalCount);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapGet(string.Empty, async Task<Ok<Response>> (
                    Handler handler, CancellationToken cancellationToken, int page = 1, int pageSize = DefaultPageSize) =>
                    TypedResults.Ok(await handler.HandleAsync(page, pageSize, cancellationToken)))
                .AllowAnonymous()
                .WithName("GetPublishedCourses")
                .WithSummary("Browse published courses (public, paged)");
    }
}
