using CourseHub.Shared;
using CourseHub.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourseHub.Billing;

public static class BillingModule
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIntegrationEventHandlersFrom(typeof(BillingModule).Assembly);
        return services;
    }

    public static IEndpointRouteBuilder MapBillingModule(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/billing")
            .WithTags("Billing")
            .MapEndpointsFrom(typeof(BillingModule).Assembly);

        return app;
    }
}
