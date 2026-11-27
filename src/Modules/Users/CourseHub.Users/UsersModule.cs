using CourseHub.Shared;
using CourseHub.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourseHub.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIntegrationEventHandlersFrom(typeof(UsersModule).Assembly);
        return services;
    }

    public static IEndpointRouteBuilder MapUsersModule(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/users")
            .WithTags("Users")
            .MapEndpointsFrom(typeof(UsersModule).Assembly);

        return app;
    }
}
