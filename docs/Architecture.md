# Architecture

**Modular Monolith**, with **Vertical Slice Architecture** inside each module and a **rich Domain** that owns the business rules.

## 1. Solution layout

```
ModularMonolith_VerticalSliceArchitecture/
├── ModularMonolith_VerticalSliceArchitecture.slnx
├── AGENTS.md, CLAUDE.md
├── docs/                      Requirements, Architecture, Conventions, Phases
├── src/
│   ├── Bootstrapper/
│   │   └── CourseHub.Api/                 Host: Program.cs, module registration, OpenAPI, auth
│   ├── Shared/
│   │   └── CourseHub.Shared/              Building blocks shared by all modules
│   └── Modules/
│       ├── Users/
│       │   ├── CourseHub.Users/
│       │   └── CourseHub.Users.Contracts/
│       ├── Courses/         (same pattern)
│       ├── Enrollment/      (same pattern)
│       ├── Billing/         (same pattern)
│       └── Notifications/   (same pattern)
└── tests/
    ├── CourseHub.<Module>.Tests/          Domain unit tests per module
    └── CourseHub.ArchitectureTests/       Module boundary rules
```

## 2. Inside a module

```
CourseHub.Courses/
├── Domain/                 Entities, value objects, enums, domain errors, domain events
│   └── Course.cs           e.g. Course.Publish() enforces CRS-03
├── Features/               One folder per use case (vertical slice)
│   ├── CreateCourse/
│   │   └── CreateCourse.cs Request + Response + Handler + Endpoint in one file
│   └── PublishCourse/
│       └── PublishCourse.cs
├── Infrastructure/
│   ├── CoursesDbContext.cs Schema "courses"
│   ├── Configurations/     IEntityTypeConfiguration<T>
│   └── Migrations/
├── IntegrationEventHandlers/   Handlers for events from other modules
└── CoursesModule.cs        AddCoursesModule(services, config) + endpoint mapping

CourseHub.Courses.Contracts/
├── Events/                 Integration events this module publishes (records)
└── ICoursesApi.cs          Query interface other modules may call
```

## 3. Dependency rules

| Project | May reference |
|---|---|
| `CourseHub.Shared` | Framework packages only |
| `CourseHub.<X>.Contracts` | `CourseHub.Shared` only (no EF, no ASP.NET) |
| `CourseHub.<X>` | `CourseHub.Shared`, its own Contracts, **other modules' Contracts** |
| `CourseHub.Api` | `CourseHub.Shared` and every module project (for registration only) |

Forbidden:
- A module referencing another module's main project (`CourseHub.Courses` → `CourseHub.Enrollment`).
- A module querying another module's tables or schema.
- Shared navigation properties or foreign keys **across** modules. Use the other module's ID only (`Guid CourseId`).

These rules are checked by `tests/CourseHub.ArchitectureTests`.

## 4. Communication between modules

1. **Synchronous queries**: through an interface in the target's Contracts (e.g. `ICoursesApi.GetCourseForEnrollment(courseId)`), implemented `internal` inside the target module.
2. **Asynchronous events**: integration events (records in Contracts), published through the in-process event bus in `CourseHub.Shared` after `SaveChanges` succeeds.

Main event flow:

```
Enrollment: EnrollmentCreated      ──▶ Billing: create invoice
Billing:    PaymentSucceeded       ──▶ Enrollment: confirm enrollment
Billing:    PaymentFailed/Expired  ──▶ Enrollment: cancel + promote waitlist
Enrollment: EnrollmentConfirmed    ──▶ Notifications: send + log message
Billing:    RefundCompleted        ──▶ Enrollment: cancel + promote waitlist
```

Template simplification: the event bus is in-memory, with no outbox. This is noted as a known limitation; an outbox can be added later.

## 5. Data

- One SQL Server database `CourseHub` on `(localdb)\MSSQLLocalDB` (Windows Authentication).
- Each module has its own `DbContext`, its own schema (`users`, `courses`, `enrollment`, `billing`, `notifications`), and its own migrations history table (`__EFMigrationsHistory` in its schema).
- Connection string name: `ConnectionStrings:CourseHub` in `appsettings.Development.json`.

## 6. Cross-cutting

- **Endpoints**: **Minimal APIs only**, with no MVC Controllers and no Carter. Each slice defines its endpoint in a class implementing `IEndpoint` (in `CourseHub.Shared`). Each module maps its endpoints under a route group, and the Api discovers them by assembly scanning.
- **Auth**:
  - ASP.NET Core Identity services with Identity bearer tokens, owned by the Users module.
  - Users has its own `Register` / `Login` / `RefreshToken` slices; `MapIdentityApi` is **not** used.
  - Authorization policies are `StudentOnly` and `InstructorOnly`. Their names live in `CourseHub.Shared.Auth`.
- **Errors**:
  - Domain methods return `Result` / `Result<T>`, and endpoints map errors with `ToProblem()`.
  - Every error response has one shape (`ApiProblem`: `code` + `traceId`, plus `errors` with camelCase keys on 400). The same shape covers errors the framework produces itself, through `ProblemDetailsCustomizer` in the Api.
  - Responses are documented by `ProducesErrors(...)` per slice and `WithStandardResponses()` on the root group. See `Conventions.md` → Errors.
- **Time**: always `TimeProvider` (needed for testable 14-day and 30-minute rules); never `DateTime.Now`.
- **Background work**: `PaymentTimeoutWorker` (`BackgroundService`) in Billing.
- **API docs**: OpenAPI document (`/openapi/v1.json`) plus the Scalar API reference (`/scalar`) in Development.
