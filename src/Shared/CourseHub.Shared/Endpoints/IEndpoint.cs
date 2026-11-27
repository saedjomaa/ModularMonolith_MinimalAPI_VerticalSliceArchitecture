using Microsoft.AspNetCore.Routing;

namespace CourseHub.Shared.Endpoints;

/// <summary>
/// A Minimal API endpoint belonging to one vertical slice.
/// <paramref name="app"/> is the module's route group (e.g. <c>/courses</c>).
/// </summary>
public interface IEndpoint
{
    void Map(IEndpointRouteBuilder app);
}
