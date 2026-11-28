using System.Security.Claims;
using CourseHub.Courses.Domain;
using CourseHub.Courses.Infrastructure;
using CourseHub.Shared.Auth;
using CourseHub.Shared.Endpoints;
using CourseHub.Shared.Handlers;
using CourseHub.Shared.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Courses.Features.PublishCourse;

/// <summary>The owning instructor publishes a course. Needs a price and an instructor (CRS-03, CRS-05).</summary>
internal static class PublishCourse
{
    internal sealed class Handler(CoursesDbContext db, TimeProvider clock) : IHandler
    {
        public async Task<Result> HandleAsync(Guid courseId, Guid actorId, CancellationToken cancellationToken)
        {
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);
            if (course is null)
            {
                return CourseErrors.NotFound;
            }

            var published = course.Publish(actorId, clock.GetUtcNow());
            if (published.IsFailure)
            {
                return published;
            }

            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return CourseErrors.ConcurrencyConflict;
            }

            return Result.Success();
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapPost("/{courseId:guid}/publish", async Task<Results<NoContent, ProblemHttpResult>> (
                    Guid courseId, ClaimsPrincipal principal, Handler handler, CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(courseId, principal.GetUserId(), cancellationToken);
                    return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
                })
                .RequireAuthorization(Policies.InstructorOnly)
                .ProducesErrors(
                    CourseErrors.NotOwner, CourseErrors.NotFound, CourseErrors.PriceRequired,
                    CourseErrors.InstructorRequired, CourseErrors.AlreadyPublished, CourseErrors.ConcurrencyConflict)
                .WithName("PublishCourse")
                .WithSummary("Publish a course (owner only; requires a price)");
    }
}
