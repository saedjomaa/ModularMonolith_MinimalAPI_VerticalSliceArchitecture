using System.ComponentModel.DataAnnotations;
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

namespace CourseHub.Courses.Features.CreateCourse;

/// <summary>
/// An instructor creates a course. It starts as a Draft (CRS-02); the price may be set later,
/// but is required to publish (CRS-03). The creator becomes the owning instructor.
/// </summary>
internal static class CreateCourse
{
    internal sealed record Request(
        [Required, MaxLength(Course.TitleMaxLength)] string Title,
        [MaxLength(Course.DescriptionMaxLength)] string? Description,
        [Range(Course.MinPrice, Course.MaxPrice)] decimal? Price,
        [Range(1, Course.MaxCapacity)] int Capacity);

    internal sealed record Response(Guid Id);

    internal sealed class Handler(CoursesDbContext db, TimeProvider clock) : IHandler
    {
        public async Task<Result<Response>> HandleAsync(Guid instructorId, Request request, CancellationToken cancellationToken)
        {
            var created = Course.Create(
                instructorId, request.Title, request.Description, request.Price, request.Capacity, clock.GetUtcNow());

            if (created.IsFailure)
            {
                return created.Error;
            }

            db.Courses.Add(created.Value);
            await db.SaveChangesAsync(cancellationToken);
            return new Response(created.Value.Id);
        }
    }

    internal sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapPost(string.Empty, async Task<Results<Created<Response>, ProblemHttpResult>> (
                    Request request, ClaimsPrincipal principal, Handler handler, CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(principal.GetUserId(), request, cancellationToken);
                    return result.IsSuccess
                        ? TypedResults.Created($"/courses/{result.Value.Id}", result.Value)
                        : result.ToProblem();
                })
                .RequireAuthorization(Policies.InstructorOnly)
                .ProducesErrors(CourseErrors.InvalidTitle, CourseErrors.InvalidDescription, CourseErrors.InvalidPrice, CourseErrors.InvalidCapacity)
                .WithName("CreateCourse")
                .WithSummary("Create a draft course (instructors only)");
    }
}
