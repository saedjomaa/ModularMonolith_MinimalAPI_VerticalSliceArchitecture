using CourseHub.Courses.Contracts;
using CourseHub.Courses.Infrastructure;
using CourseHub.Shared;
using CourseHub.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourseHub.Courses;

public static class CoursesModule
{
    public static IServiceCollection AddCoursesModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CourseHub")
            ?? throw new InvalidOperationException("Connection string 'CourseHub' is not configured.");

        services.AddDbContext<CoursesDbContext>(options => options.UseSqlServer(
            connectionString,
            sql => sql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, CoursesDbContext.Schema)));

        services.AddHostedService<CoursesDevelopmentMigrator>();
        services.AddScoped<ICoursesApi, CoursesApi>();

        services.AddValidation();
        services.AddHandlersFrom(typeof(CoursesModule).Assembly);
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
