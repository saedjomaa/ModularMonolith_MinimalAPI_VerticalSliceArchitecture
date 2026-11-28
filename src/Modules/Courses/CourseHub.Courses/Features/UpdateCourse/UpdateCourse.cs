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
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Courses.Features.UpdateCourse;

/// <summary>
/// The owning instructor changes title, description, price and capacity (CRS-04, CRS-05).
/// A published course must keep its price (CRS-03).
/// </summary>
internal static class UpdateCourse
{
    internal sealed record Request(
        [Required, MaxLength(Course.TitleMaxLength)] string Title,
        [MaxLength(Course.DescriptionMaxLength)] string? Description,
        [Range(Course.MinPrice, Course.MaxPrice)] decimal? Price,
        [Range(1, Course.MaxCapacity)] int Capacity);

    internal sealed class Handler(CoursesDbContext db) : IHandler
    {
        public async Task<Result> HandleAsync(Guid courseId, Guid actorId, Request request, CancellationToken cancellationToken)
        {
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);
            if (course is null)
            {
                return CourseErrors.NotFound;
            }

            var updated = course.Update(actorId, request.Title, request.Description, request.Price, request.Capacity);
            if (updated.IsFailure)
            {
                return updated;
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
            app.MapPut("/{courseId:guid}", async Task<Results<NoContent, ProblemHttpResult>> (
                    Guid courseId, Request request, ClaimsPrincipal principal, Handler handler, CancellationToken cancellationToken) =>
                {
                    var result = await handler.HandleAsync(courseId, principal.GetUserId(), request, cancellationToken);
                    return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
                })
                .RequireAuthorization(Policies.InstructorOnly)
                .ProducesErrors(
                    CourseErrors.InvalidTitle, CourseErrors.InvalidDescription, CourseErrors.InvalidPrice, CourseErrors.InvalidCapacity,
                    CourseErrors.NotOwner, CourseErrors.NotFound, CourseErrors.PriceRequired, CourseErrors.ConcurrencyConflict)
                .WithName("UpdateCourse")
                .WithSummary("Update a course's title, description, price and capacity (owner only)");
    }
}
