using System.Reflection;
using Microsoft.AspNetCore.Routing;

namespace CourseHub.Shared.Endpoints;

public static class EndpointExtensions
{
    /// <summary>
    /// Finds every <see cref="IEndpoint"/> in <paramref name="assembly"/> (internal types included)
    /// and maps it onto <paramref name="app"/>.
    /// </summary>
    public static IEndpointRouteBuilder MapEndpointsFrom(this IEndpointRouteBuilder app, Assembly assembly)
    {
        var endpointTypes = assembly.DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } && type.IsAssignableTo(typeof(IEndpoint)));

        foreach (var type in endpointTypes)
        {
            var endpoint = (IEndpoint)Activator.CreateInstance(type, nonPublic: true)!;
            endpoint.Map(app);
        }

        return app;
    }
}
