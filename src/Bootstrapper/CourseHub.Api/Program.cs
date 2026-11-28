using CourseHub.Api.ErrorHandling;
using CourseHub.Api.OpenApi;
using CourseHub.Billing;
using CourseHub.Courses;
using CourseHub.Enrollment;
using CourseHub.Notifications;
using CourseHub.Shared;
using CourseHub.Shared.Endpoints;
using CourseHub.Users;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCourseHubOpenApi();
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = ProblemDetailsCustomizer.Apply);
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
    app.MapScalarApiReference(options => options
        .WithTitle("CourseHub API")
        .AddPreferredSecuritySchemes(OpenApiConfiguration.BearerSchemeName));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

// One root group so every module's endpoints get the standard responses (400/401/403/500) documented.
app.MapGroup(string.Empty)
   .WithStandardResponses()
   .MapUsersModule()
   .MapCoursesModule()
   .MapEnrollmentModule()
   .MapBillingModule()
   .MapNotificationsModule();

app.Run();

/// <summary>Exposed for integration tests (WebApplicationFactory).</summary>
public partial class Program;
