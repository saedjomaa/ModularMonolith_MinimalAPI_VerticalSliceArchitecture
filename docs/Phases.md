# Build phases

Arabic version: `../../Phases.txt`.

## Rules

- Work on **one phase at a time**. Do not start the next phase without the user's explicit approval.
- Each phase ends with: a successful `dotnet build`, its verification steps passed, and a short report to the user (what was done, what was verified, any open questions).
- Update the **Status** column below at the end of each phase.

| # | Phase | Status |
|---|---|---|
| 0 | Skeleton | Done (2026-11-27) |
| 1 | Users | Not started |
| 2 | Courses | Not started |
| 3 | Enrollment | Not started |
| 4 | Billing | Not started |
| 5 | Notifications | Not started |
| 6 | Tests and finishing | Not started |

## Phase 0 — Skeleton

**Goal:** a solution that builds and runs, with no business logic.

- Create `ModularMonolith_VerticalSliceArchitecture.slnx`, `Directory.Build.props`, `Directory.Packages.props`, `.gitignore`, `.editorconfig`.
- Create `CourseHub.Api` with OpenAPI + Swagger UI, and `appsettings.Development.json` with the LocalDB connection string.
- Create `CourseHub.Shared`, containing:
  - `Entity`, `AggregateRoot`, `Result`, `Error`.
  - `IEndpoint` with assembly scanning.
  - `IEventBus` (in-memory) and `IIntegrationEventHandler<T>`.
  - `ProblemDetails` mapping for `Result`.
- Create empty module and Contracts projects for all five modules, each with an `Add<X>Module` extension.
- Create an empty `CourseHub.ArchitectureTests` project with the dependency rules from `Architecture.md` §3.

**Verify:** `dotnet build` passes, `dotnet test` passes (architecture tests), the API runs, and `/swagger` opens.

**Result:**
- Build: 0 warnings, 0 errors.
- Tests: 21 architecture tests pass. A deliberate violation was confirmed to fail, then removed.
- Runtime: `/health` returns 200, `/openapi/v1.json` returns 200, and `/swagger` opens.
- Notes for later phases:
  - Route groups: `/users`, `/courses`, `/enrollments`, `/billing`, `/notifications`.
  - The built-in Minimal API validation (`AddValidation()`) is not registered yet. It will be added in Phase 2 together with the first validated `Request`.
  - Empty `Domain/`, `Features/` and `Infrastructure/` folders are not created until they hold code.

## Phase 1 — Users

- Identity with `ApplicationUser`, a `UsersDbContext` (schema `users`), and `MapIdentityApi`.
- Roles `Student` / `Instructor`; new users get `Student` (USR-03).
- Seed a development instructor (USR-04).
- Student profile slice (USR-05). Add `IUsersApi` in Contracts.
- Policies `StudentOnly` and `InstructorOnly`.
- First migration.

**Verify:** register, log in and get a token; log in as the seeded instructor; a student gets 403 on an instructor endpoint.

## Phase 2 — Courses

- `Course` aggregate (CRS-01…CRS-04) and `CoursesDbContext` (schema `courses`).
- Slices: `CreateCourse`, `UpdateCourse` (price, capacity), `PublishCourse`, `GetMyCourses`, `GetPublishedCourses`, `GetCourse`.
- Ownership check (CRS-05) and published-only browsing (CRS-06).
- `ICoursesApi` in Contracts (used by Enrollment).

**Verify:** publishing without a price returns 409; publishing with a price succeeds; another instructor gets 403.

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
