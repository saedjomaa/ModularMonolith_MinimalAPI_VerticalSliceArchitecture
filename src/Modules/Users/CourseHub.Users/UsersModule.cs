using CourseHub.Shared;
using CourseHub.Shared.Auth;
using CourseHub.Shared.Endpoints;
using CourseHub.Users.Contracts;
using CourseHub.Users.Domain;
using CourseHub.Users.Infrastructure;
using CourseHub.Users.Infrastructure.Seeding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CourseHub.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CourseHub")
            ?? throw new InvalidOperationException("Connection string 'CourseHub' is not configured.");

        services.AddDbContext<UsersDbContext>(options => options.UseSqlServer(
            connectionString,
            sql => sql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, UsersDbContext.Schema)));

        // Identity bearer tokens (the same tokens MapIdentityApi uses), without MapIdentityApi's endpoints.
        services.AddAuthentication(IdentityConstants.BearerScheme)
            .AddBearerToken(IdentityConstants.BearerScheme);

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.StudentOnly, policy => policy.RequireRole(Roles.Student))
            .AddPolicy(Policies.InstructorOnly, policy => policy.RequireRole(Roles.Instructor));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true; // USR-02
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<UsersDbContext>()
            .AddSignInManager();

        services.Configure<SeedInstructorOptions>(configuration.GetSection(SeedInstructorOptions.SectionName));
        services.AddHostedService<UsersDevelopmentInitializer>();

        services.AddScoped<IUsersApi, UsersApi>();

        services.AddValidation();
        services.AddHandlersFrom(typeof(UsersModule).Assembly);
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
