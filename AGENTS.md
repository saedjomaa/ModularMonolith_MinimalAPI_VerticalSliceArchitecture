# AGENTS.md

Instructions for AI coding agents working in this repository.

## What this is

**CourseHub** is a course platform backend: a .NET 11 Web API built as a **Modular Monolith** with **Vertical Slice Architecture** inside each module.

## Read before coding

1. `docs/Requirements.md` — business rules, each with an ID (CRS-03, ENR-04, ...).
2. `docs/Architecture.md` — solution layout, module structure, dependency rules, events.
3. `docs/Conventions.md` — code style, slice shape, packages, API and test rules.
4. `docs/Phases.md` — the current phase and its scope.

## Working rules

- **Work only on the current phase** in `docs/Phases.md`. Never start the next phase without the user's explicit approval.
- At the end of a phase:
  - Run `dotnet build` and `dotnet test`.
  - Run the phase's verification steps.
  - Update the status table in `docs/Phases.md`.
  - Report to the user: what was done, what was verified, and open questions.
- **Business rules live in the Domain**, never in handlers or endpoints. Reference the rule ID in a comment and in its test.
- **Respect module boundaries**:
  - Modules talk only through `*.Contracts` (query interfaces and integration events).
  - No cross-module project references, table access or foreign keys.
- If a requirement is unclear or missing, **ask the user**. Don't guess. Record the decision in `docs/Requirements.md`.
- Don't add NuGet packages beyond those listed in `docs/Conventions.md` without asking.
- Keep these docs up to date when a decision changes.

## Commands

Run all commands from this folder:

```powershell
dotnet tool restore   # installs dotnet-ef (local tool, dotnet-tools.json)
dotnet build
dotnet test
dotnet run --project src/Bootstrapper/CourseHub.Api
dotnet ef migrations add <Name> --project src/Modules/<X>/CourseHub.<X> --startup-project src/Bootstrapper/CourseHub.Api --context <X>DbContext --output-dir Infrastructure/Migrations
dotnet ef database update --project src/Modules/<X>/CourseHub.<X> --startup-project src/Bootstrapper/CourseHub.Api --context <X>DbContext
```

## Environment

- Windows, PowerShell, .NET SDK 11.0.100.
- SQL Server LocalDB `(localdb)\MSSQLLocalDB` with Windows Authentication. The database is `CourseHub`.
- The user communicates in Arabic. Reply in Arabic unless asked otherwise. Code, identifiers and these docs are in English.
