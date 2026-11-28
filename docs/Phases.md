# Build phases

Arabic version: `../../Phases.txt`.

## Rules

- Work on **one phase at a time**. Do not start the next phase without the user's explicit approval.
- Each phase ends with: a successful `dotnet build`, its verification steps passed, and a short report to the user (what was done, what was verified, any open questions).
- Update the **Status** column below at the end of each phase.

| # | Phase | Status |
|---|---|---|
| 0 | Skeleton | Done (2026-11-27) |
| 1 | Users | Done (2026-11-27) |
| 2 | Courses | Done (2026-11-28) |
| 3 | Enrollment | Not started |
| 4 | Billing | Not started |
| 5 | Notifications | Not started |
| 6 | Tests and finishing | Not started |

## Phase 0 — Skeleton

**Goal:** a solution that builds and runs, with no business logic.

- Create `ModularMonolith_VerticalSliceArchitecture.slnx`, `Directory.Build.props`, `Directory.Packages.props`, `.gitignore`, `.editorconfig`.
- Create `CourseHub.Api` with OpenAPI + Scalar, and `appsettings.Development.json` with the LocalDB connection string.
- Create `CourseHub.Shared`, containing:
  - `Entity`, `AggregateRoot`, `Result`, `Error`.
  - `IEndpoint` with assembly scanning.
  - `IEventBus` (in-memory) and `IIntegrationEventHandler<T>`.
  - `ProblemDetails` mapping for `Result`.
- Create empty module and Contracts projects for all five modules, each with an `Add<X>Module` extension.
- Create an empty `CourseHub.ArchitectureTests` project with the dependency rules from `Architecture.md` §3.

**Verify:** `dotnet build` passes, `dotnet test` passes (architecture tests), the API runs, and `/scalar` opens.

**Result:**
- Build: 0 warnings, 0 errors.
- Tests: 21 architecture tests pass. A deliberate violation was confirmed to fail, then removed.
- Runtime: `/health` returns 200, `/openapi/v1.json` returns 200, and `/scalar` opens. Swagger UI was replaced by Scalar after review.
- Notes for later phases:
  - Route groups: `/users`, `/courses`, `/enrollments`, `/billing`, `/notifications`.
  - The built-in Minimal API validation (`AddValidation()`) was not registered in Phase 0. It was added in Phase 1.
  - Empty `Domain/`, `Features/` and `Infrastructure/` folders are not created until they hold code.

## Phase 1 — Users

- Identity with `ApplicationUser : IdentityUser<Guid>` and a `UsersDbContext` (schema `users`).
- Identity **services and bearer tokens**, but **not** `MapIdentityApi`: its `/register` can't take the full name and phone number or assign a role (decided with the user).
- Custom slices:
  - `Register` (USR-01…03, 05)
  - `Login`
  - `RefreshToken`
  - `GetMyProfile`
  - `UpdateMyProfile` (USR-05)
  - `PromoteToInstructor` (USR-06)
- Roles `Student` / `Instructor`, seeded by the migration. New users get `Student` (USR-03).
- Seed a development instructor (USR-04).
- Add `IUsersApi` in Contracts.
- Policies `StudentOnly` and `InstructorOnly`.
- First migration.

**Verify:** register, log in and get a token; log in as the seeded instructor; a student gets 403 on an instructor endpoint.

**Result:**
- Build: 0 warnings, 0 errors. Tests: 21 architecture tests pass.
- Database: 7 Identity tables plus `__EFMigrationsHistory` in schema `users`, with the `Student` and `Instructor` roles.
- 19 of 19 HTTP checks pass:
  - register returns 201; a duplicate email returns 409; invalid input returns 400 with field errors
  - login returns 200; a wrong password returns 401
  - `/users/me` returns 200 with a token and 401 without one
  - a profile update returns 204 and is persisted
  - a student calling the promote endpoint gets 403
  - the seeded instructor can log in
  - promote returns 204; promoting again returns 409; an unknown user returns 404
  - after a refresh, the promoted user's token carries the `Instructor` role
  - an invalid refresh token returns 401
- Scalar shows the 6 Users endpoints and a Bearer token field.
- Added to Shared:
  - `Auth/Roles`, `Auth/Policies`, `Auth/ClaimsPrincipalExtensions.GetUserId()`
  - `Handlers/IHandler` with `AddHandlersFrom(assembly)`
- Tooling: `dotnet-ef` 11.0.0 is installed as a local tool (`dotnet-tools.json`). Run `dotnet tool restore` after cloning.
- Development only: on startup the API applies the Users migrations and seeds `instructor@coursehub.local` (password in `appsettings.Development.json`).
- Each module calls `services.AddValidation()` in its own `Add<X>Module`, so the validation source generator sees that module's `Request` records.
- The architecture visibility test ignores the `*.Infrastructure.Migrations` namespace, because EF generates public migration classes.

**After review: unified error contract** (2026-11-28)
- One error shape for all non-2xx responses: `code` + `traceId` + `instance`, plus camelCase `errors` on 400. Framework errors are included.
- Identity password and email errors now appear as field errors.
- `/users/refresh` returns `Users.InvalidRefreshToken` instead of an empty 401.
- Every endpoint documents all its responses: `ProducesErrors(...)` per slice, and `WithStandardResponses()` adds 400/401/403/500.
- Verified:
  - 9 of 9 error-shape checks pass: validation, weak password, domain field error, 409, 401 wrong password, 401 no token, 403, 404 unknown route, 401 bad refresh token.
  - The 19 Phase 1 checks still pass.
  - OpenAPI lists, for example, 204/400/401/403/404/409/500 for promote, with codes in the descriptions.

## Phase 2 — Courses

- `Course` aggregate (CRS-01…CRS-04) and `CoursesDbContext` (schema `courses`).
- Slices: `CreateCourse`, `UpdateCourse` (price, capacity), `PublishCourse`, `GetMyCourses`, `GetPublishedCourses`, `GetCourse`.
- Ownership check (CRS-05) and published-only browsing (CRS-06).
- `ICoursesApi` in Contracts (used by Enrollment).

**Verify:** publishing without a price returns 409; publishing with a price succeeds; another instructor gets 403.

**Result:**
- `Course` aggregate holds every rule:
  - `Create`, `Update` and `Publish` with owner checks (CRS-05)
  - a price is required to publish, and a published course can't lose its price (CRS-03)
  - capacity is between 1 and 10,000 (CRS-04)
  - price is between 0.01 and 1,000,000, rounded to 2 decimals
  - `IsVisibleTo` (CRS-06)
  - `rowversion` for optimistic concurrency, which returns 409 `Courses.ConcurrencyConflict`
- Slices:
  - `POST /courses` (instructor)
  - `PUT /courses/{id}` (owner)
  - `POST /courses/{id}/publish` (owner)
  - `GET /courses/mine` (instructor)
  - `GET /courses` (public, paged, published only)
  - `GET /courses/{id}` (public; a draft returns 404 to everyone except its owner)
- First cross-module query: the catalog and course details show the instructor's name through `IUsersApi` (`Courses` → `Users.Contracts`).
- `ICoursesApi.GetCourseAsync` in Contracts returns id, title, instructor, price, capacity and published flag, for Enrollment and Billing.
- Added `ClaimsPrincipal.TryGetUserId()` to Shared, for anonymous-friendly endpoints.
- Database: `courses.Courses` plus its own `__EFMigrationsHistory`. Migrations are applied on startup in Development.
- Verified:
  - build has 0 warnings; 21 architecture tests pass
  - 23 of 23 Phase 2 HTTP checks pass: CRS-01…06, 401/403/404/409, the catalog, visibility, instructor names, and the OpenAPI responses for publish
  - regressions all pass: Phase 1 19 of 19, error shape 9 of 9, login 4 of 4
- Not verified over HTTP: the concurrency conflict (409 `Courses.ConcurrencyConflict`). It needs two simultaneous writes; it will be covered by tests in Phase 6.
- Decisions to confirm:
  - a published course can still be edited (title, description, price, capacity)
  - price must be greater than 0 (no free courses)
  - the catalog is public (no sign-in needed)

## Phase 3 — Enrollment

- `CourseSeats` aggregate per course, holding capacity, enrollments and the waitlist (ENR-01…ENR-06).
- Concurrency control with `rowversion` (ENR-02).
- Slices: `EnrollInCourse`, `CancelEnrollment`, `GetMyEnrollments`, `GetCourseStudents` (instructor only, ENR-07).
- Publish `EnrollmentCreated`, `EnrollmentConfirmed`, `EnrollmentCancelled`.

**Verify:**
- Fill the course; the next student is waitlisted.
- Cancel an enrollment; the first waitlisted student gets the seat.
- Parallel enroll requests never exceed capacity.

## Phase 4 — Billing

- `Invoice` (BIL-01, BIL-02), `Payment` (BIL-03, BIL-04), and `Refund` (BIL-05).
- `FakePaymentGateway` with a configurable outcome (BIL-07).
- `PaymentTimeoutWorker` for the 30-minute expiry.
- Handle `EnrollmentCreated` by creating an invoice. Publish `PaymentSucceeded`, `PaymentFailed`, `PaymentExpired` and `RefundCompleted`.
- Enrollment handles these events (BIL-06, and the open questions in `Requirements.md` §4).

**Verify:**
- A successful payment confirms the enrollment.
- A failed payment does not confirm it.
- Editing a paid invoice returns 409.
- A refund after day 14 returns 409.
- A payment pending for more than 30 minutes expires.

## Phase 5 — Notifications

- Handle `EnrollmentConfirmed`, send through `FakeMessageSender`, and store a `MessageLog` (NTF-01…NTF-03).
- Slice: `GetMyMessages`.

**Verify:** a confirmed enrollment produces a log entry.

## Phase 6 — Tests and finishing

- Domain unit tests for every rule ID.
- An `.http` file with the full scenario: register → publish course → enroll → pay → confirmed → message logged → refund.
- A README with how to run the project.

**Verify:** `dotnet test` is all green, and the `.http` scenario passes end to end.
