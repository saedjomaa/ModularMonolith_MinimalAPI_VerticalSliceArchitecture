# Coding conventions

## Language and framework

- .NET 11 (SDK 11.0.100), C# latest, `Nullable` enabled, `ImplicitUsings` enabled, `TreatWarningsAsErrors` on.
- Shared settings live in `Directory.Build.props`; package versions in `Directory.Packages.props` (central package management).
- File-scoped namespaces. Namespace = folder path (`CourseHub.Courses.Features.PublishCourse`).

## Packages (keep the list short)

- `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.AspNetCore.OpenApi` + `Swashbuckle.AspNetCore.SwaggerUI`
- Tests: `xunit`, `Shouldly` (or `FluentAssertions` only if the user prefers), `NetArchTest.Rules`
- **No MediatR, no AutoMapper, no Carter.** Handlers are plain classes. Mapping is written by hand. Endpoints are discovered through our own `IEndpoint`.
- Ask the user before adding any other package.

## Vertical slice file

One file per feature, holding:

```csharp
internal static class PublishCourse
{
    public sealed record Request(...);
    public sealed record Response(...);

    internal sealed class Handler(CoursesDbContext db, TimeProvider clock) { ... }

    internal sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapPost("/{id:guid}/publish", ...)   // inside the module's "/courses" group
               .RequireAuthorization("InstructorOnly");
    }
}
```

- Feature folder and class are named as **verb + noun** (`EnrollInCourse`, `RefundPayment`).
- Validation of input shape goes in the slice. **Business rules go in the Domain** — never in the handler.
- The handler loads the aggregate, calls one domain method, saves, and returns.

## Domain

- Entities have private setters and a private parameterless constructor for EF.
- Creation goes through static factories (`Course.Create(...)`) that return `Result<Course>`.
- State changes go through methods (`Publish()`, `Cancel()`, `Refund(now)`) that return `Result`.
- Errors are static fields in `<Entity>Errors` (e.g. `CourseErrors.PriceRequired`) with a code like `Courses.PriceRequired`.
- Reference the requirement ID next to the rule: `// CRS-03`.
- IDs are `Guid` (version 7: `Guid.CreateVersion7()`).
- Money is `decimal` with precision `(18,2)`.

## Visibility

- Everything in a module project is `internal`, except the `<X>Module` registration class.
- Contracts are `public`.

## API

- **Minimal APIs only.** No `Controller` / `ControllerBase` classes, and no `AddControllers()`.
- Handlers return `TypedResults` (e.g. `Results<Ok<Response>, NotFound, ProblemDetails>`) so OpenAPI shows the exact responses.
- Input validation uses the built-in Minimal API validation (`builder.Services.AddValidation()` with DataAnnotations on `Request` records).
- Each module maps its endpoints into a route group (`app.MapGroup("/courses").WithTags("Courses")`).
- Routes are plural and lowercase. Module groups: `/users`, `/courses`, `/enrollments`, `/billing` (e.g. `/billing/invoices`), `/notifications`.
- Errors are returned as `ProblemDetails`: 400 for validation, 404 for not found, 409 for rule conflicts, 403 for ownership.
- Every endpoint has `.WithTags(<Module>)` and an explicit authorization policy (or `.AllowAnonymous()`).

## Tests

- Test names follow `Method_State_Expected`, and each references the rule ID (`Publish_WithoutPrice_Fails // CRS-03`).
- Every rule in `docs/Requirements.md` has at least one domain unit test.
