using CourseHub.Shared;
using CourseHub.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourseHub.Notifications;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIntegrationEventHandlersFrom(typeof(NotificationsModule).Assembly);
        return services;
    }

    public static IEndpointRouteBuilder MapNotificationsModule(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/notifications")
            .WithTags("Notifications")
            .MapEndpointsFrom(typeof(NotificationsModule).Assembly);

        return app;
    }
}
