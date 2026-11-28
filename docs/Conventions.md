# Coding conventions

## Language and framework

- .NET 11 (SDK 11.0.100), C# latest, `Nullable` enabled, `ImplicitUsings` enabled, `TreatWarningsAsErrors` on.
- Shared settings live in `Directory.Build.props`; package versions in `Directory.Packages.props` (central package management).
- File-scoped namespaces. Namespace = folder path (`CourseHub.Courses.Features.PublishCourse`).

## Packages (keep the list short)

- `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.AspNetCore.OpenApi` + `Scalar.AspNetCore` (API reference UI; no Swashbuckle)
- Tests: `xunit`, `Shouldly` (or `FluentAssertions` only if the user prefers), `NetArchTest.Rules`
- **No MediatR, no AutoMapper, no Carter.** Handlers are plain classes. Mapping is written by hand. Endpoints are discovered through our own `IEndpoint`.
- Ask the user before adding any other package.

## Vertical slice file

One file per feature, holding:

```csharp
internal static class PublishCourse
{
    internal sealed record Request(...);     // DataAnnotations for input shape
    internal sealed record Response(...);

    internal sealed class Handler(CoursesDbContext db, TimeProvider clock) : IHandler { ... }

    internal sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app) =>
            app.MapPost("/{id:guid}/publish", ...)   // inside the module's "/courses" group
               .RequireAuthorization(Policies.InstructorOnly)
               .WithName("PublishCourse");
    }
}
```

- Every nested type is `internal` (the architecture tests treat nested `public` types as public).
- The handler implements `IHandler`. It is registered automatically by `AddHandlersFrom(assembly)`, and the endpoint receives it as a lambda parameter.
- Policy and role names come from `CourseHub.Shared.Auth` (`Policies`, `Roles`). The current user's id comes from `principal.GetUserId()`.

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
- Endpoints return `TypedResults` (e.g. `Results<Ok<Response>, ProblemHttpResult>`) so OpenAPI shows the success response. Error responses are declared with `.ProducesErrors(...)` (see Errors below).
- Input validation uses the built-in Minimal API validation: DataAnnotations on `Request` records. **Each module calls `services.AddValidation()` in its own `Add<X>Module`**, so the source generator sees that module's records. Invalid input returns 400 `ValidationProblem` with errors per field.
- Each module maps its endpoints into a route group (`app.MapGroup("/courses").WithTags("Courses")`).
- Routes are plural and lowercase. Module groups: `/users`, `/courses`, `/enrollments`, `/billing` (e.g. `/billing/invoices`), `/notifications`.
- Every endpoint has `.WithTags(<Module>)` (from its group) and an explicit authorization policy (or `.AllowAnonymous()`).

## Errors (one shape for every non-2xx response)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "An account with this email already exists.",
  "instance": "/users/register",
  "code": "Users.EmailTaken",
  "traceId": "00-…",
  "errors": { "password": ["…"] }
}
```

- `errors` appears **only on 400**. Its keys are **camelCase** request field names; `""` means "not tied to a field".
- Documented as `ApiProblem` / `ApiValidationProblem` (`CourseHub.Shared.Results`).
- Status by `ErrorType`: Validation 400, Unauthorized 401, Forbidden 403, NotFound 404, Conflict 409, Unexpected 500.
- Framework-produced errors get standard codes (`StandardErrors`):
  - `Validation.Failed`
  - `Auth.Unauthorized`
  - `Auth.Forbidden`
  - `Route.NotFound`
  - `Server.Unexpected`
- Module errors use `<Module>.<Reason>` (e.g. `Users.EmailTaken`) and live in `<Entity>Errors`.
- A field-level business error uses `Error.Validation(code, description, "fieldName")`.
- **Every slice declares every business error it can return** with `.ProducesErrors(XErrors.A, XErrors.B, …)`. This adds the documented response and lists the codes in OpenAPI.
- 400 (body), 401, 403 (policy) and 500 are added automatically by `WithStandardResponses()` on the root group in `Program.cs`. Don't repeat them.
- Never build `ProblemDetails` by hand in a slice. Return `result.ToProblem()`.

## Tests

- Test names follow `Method_State_Expected`, and each references the rule ID (`Publish_WithoutPrice_Fails // CRS-03`).
- Every rule in `docs/Requirements.md` has at least one domain unit test.
