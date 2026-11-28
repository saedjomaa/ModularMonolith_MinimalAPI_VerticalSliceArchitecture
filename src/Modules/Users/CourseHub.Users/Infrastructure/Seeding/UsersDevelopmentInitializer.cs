using CourseHub.Shared.Auth;
using CourseHub.Users.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CourseHub.Users.Infrastructure.Seeding;

/*
 يعمل عند تشغيل التطبيق في بيئة Development فقط:
 1) يطبّق الـ Migrations الخاصة بوحدة Users على قاعدة البيانات.
 2) ينشئ حساب المدرّب الافتراضي إذا لم يكن موجودًا (USR-04).
 الأدوار نفسها تُنشأ عبر الـ Migration (RoleConfiguration) وليس هنا.
 */
/// <summary>
/// Development only: applies the Users migrations and seeds the default instructor (USR-04).
/// Roles are seeded by the migration itself, so they exist in every environment.
/// </summary>
internal sealed partial class UsersDevelopmentInitializer(
    IServiceScopeFactory scopeFactory,
    IHostEnvironment environment,
    IOptions<SeedInstructorOptions> seedOptions,
    TimeProvider clock,
    ILogger<UsersDevelopmentInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await db.Database.MigrateAsync(cancellationToken);

        await SeedInstructorAsync(scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedInstructorAsync(UserManager<ApplicationUser> userManager)
    {
        var seed = seedOptions.Value;
        if (string.IsNullOrWhiteSpace(seed.Email) || string.IsNullOrWhiteSpace(seed.Password))
        {
            LogSeedSkipped();
            return;
        }

        if (await userManager.FindByEmailAsync(seed.Email) is not null)
        {
            return;
        }

        var created = ApplicationUser.Create(seed.Email, seed.FullName, seed.PhoneNumber, clock.GetUtcNow());
        if (created.IsFailure)
        {
            throw new InvalidOperationException($"Invalid seed instructor settings: {created.Error.Description}");
        }

        var user = created.Value;
        var result = await userManager.CreateAsync(user, seed.Password);
        if (result.Succeeded)
        {
            result = await userManager.AddToRoleAsync(user, Roles.Instructor);
        }

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not seed the instructor: {string.Join(" ", result.Errors.Select(e => e.Description))}");
        }

        LogSeeded(seed.Email);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded development instructor {Email}")]
    private partial void LogSeeded(string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No seed instructor configured (Users:SeedInstructor); skipping")]
    private partial void LogSeedSkipped();
}
