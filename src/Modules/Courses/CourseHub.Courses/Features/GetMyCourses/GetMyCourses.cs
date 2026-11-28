using System.Security.Claims;
using CourseHub.Courses.Infrastructure;
using CourseHub.Shared.Auth;
using CourseHub.Shared.Endpoints;
using CourseHub.Shared.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Courses.Features.GetMyCourses;

/// <summary>Lists the signed-in instructor's own courses, drafts included.</summary>
internal static class GetMyCourses
{
    internal sealed record Item(
        Guid Id,
        string Title,
        string Status,
        decimal? Price,
        int Capacity,
        DateTimeOffset CreatedAt,
        DateTimeOffset? PublishedAt);

    internal sealed class Handler(CoursesDbContext db) : IHandler
    {
        public async Task<IReadOnlyList<Item>> HandleAsync(Guid instructorId, CancellationToken cancellationToken)
        {
            var courses = await db.Courses
                .AsNoTracking()
                .Where(c => c.InstructorId == instructorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);

            return [.. courses.Select(c => new Item(c.Id, c.Title, c.Status.ToString(), c.Price, c.Capacity, c.CreatedAt, c.PublishedAt))];
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapGet("/mine", async Task<Ok<IReadOnlyList<Item>>> (
                    ClaimsPrincipal principal, Handler handler, CancellationToken cancellationToken) =>
                    TypedResults.Ok(await handler.HandleAsync(principal.GetUserId(), cancellationToken)))
                .RequireAuthorization(Policies.InstructorOnly)
                .WithName("GetMyCourses")
                .WithSummary("List my courses, drafts included (instructors only)");
    }
}
