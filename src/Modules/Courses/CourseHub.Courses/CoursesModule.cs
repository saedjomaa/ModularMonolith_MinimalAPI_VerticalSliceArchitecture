using CourseHub.Shared;
using CourseHub.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourseHub.Courses;

public static class CoursesModule
{
    public static IServiceCollection AddCoursesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIntegrationEventHandlersFrom(typeof(CoursesModule).Assembly);
        return services;
    }

    public static IEndpointRouteBuilder MapCoursesModule(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/courses")
            .WithTags("Courses")
            .MapEndpointsFrom(typeof(CoursesModule).Assembly);

        return app;
    }
}
