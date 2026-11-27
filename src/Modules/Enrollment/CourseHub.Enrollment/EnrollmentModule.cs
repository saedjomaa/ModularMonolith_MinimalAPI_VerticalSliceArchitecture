using CourseHub.Shared;
using CourseHub.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourseHub.Enrollment;

public static class EnrollmentModule
{
    public static IServiceCollection AddEnrollmentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIntegrationEventHandlersFrom(typeof(EnrollmentModule).Assembly);
        return services;
    }

    public static IEndpointRouteBuilder MapEnrollmentModule(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/enrollments")
            .WithTags("Enrollment")
            .MapEndpointsFrom(typeof(EnrollmentModule).Assembly);

        return app;
    }
}
