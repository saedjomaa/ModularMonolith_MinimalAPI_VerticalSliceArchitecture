using System.Security.Claims;
using CourseHub.Courses.Domain;
using CourseHub.Courses.Infrastructure;
using CourseHub.Shared.Auth;
using CourseHub.Shared.Endpoints;
using CourseHub.Shared.Handlers;
using CourseHub.Shared.Results;
using CourseHub.Users.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Courses.Features.GetCourse;

/// <summary>
/// Course details. Published courses are public; a draft is visible only to its owner,
/// and returns 404 to everyone else so its existence is not revealed (CRS-06).
/// </summary>
internal static class GetCourse
{
    internal sealed record Response(
        Guid Id,
        string Title,
        string? Description,
        decimal? Price,
        int Capacity,
        string Status,
        Guid InstructorId,
        string? InstructorName,
        DateTimeOffset CreatedAt,
        DateTimeOffset? PublishedAt);

    internal sealed class Handler(CoursesDbContext db, IUsersApi usersApi) : IHandler
    {
        public async Task<Result<Response>> HandleAsync(Guid courseId, Guid? viewerId, CancellationToken cancellationToken)
        {
            var course = await db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);
            if (course is null || !course.IsVisibleTo(viewerId))
            {
                return CourseErrors.NotFound;
            }

            var instructor = await usersApi.GetUserAsync(course.InstructorId, cancellationToken);

            return new Response(
                course.Id, course.Title, course.Description, course.Price, course.Capacity, course.Status.ToString(),
                course.InstructorId, instructor?.FullName, course.CreatedAt, course.PublishedAt);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapGet("/{courseId:guid}", async Task<Results<Ok<Response>, ProblemHttpResult>> (
                    Guid courseId, ClaimsPrincipal principal, Handler handler, CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(courseId, principal.TryGetUserId(), cancellationToken);
                    return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblem();
                })
                .AllowAnonymous()
                .ProducesErrors(CourseErrors.NotFound)
                .WithName("GetCourse")
                .WithSummary("Get a course (drafts are visible to their owner only)");
    }
}
