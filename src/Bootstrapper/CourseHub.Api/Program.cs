using CourseHub.Billing;
using CourseHub.Courses;
using CourseHub.Enrollment;
using CourseHub.Notifications;
using CourseHub.Shared;
using CourseHub.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddSharedServices();

builder.Services
    .AddUsersModule(builder.Configuration)
    .AddCoursesModule(builder.Configuration)
    .AddEnrollmentModule(builder.Configuration)
    .AddBillingModule(builder.Configuration)
    .AddNotificationsModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "CourseHub API v1"));
}

app.MapHealthChecks("/health");

app.MapUsersModule()
   .MapCoursesModule()
   .MapEnrollmentModule()
   .MapBillingModule()
   .MapNotificationsModule();

app.Run();

/// <summary>Exposed for integration tests (WebApplicationFactory).</summary>
public partial class Program;
