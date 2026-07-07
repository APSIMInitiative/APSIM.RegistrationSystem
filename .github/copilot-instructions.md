# Copilot Instructions for APSIM.RegistrationSystem

These instructions help GitHub Copilot (and coding agents) make accurate changes in this repository.

## Repository overview

- Solution: `RegistrationSystem.sln`
- Target framework: `.NET 10` (`net10.0`)
- Main projects:
  - `RegistrationShared`: shared enums/models used by API and web app
  - `RegistrationWebAPI`: ASP.NET Core minimal API + EF Core + SQLite
  - `RegistrationWebApp`: Blazor Server app (interactive server render mode)
  - `Tests`: xUnit tests (API integration + Playwright UI smoke tests)

## Build and run

Prefer these commands from the repo root unless otherwise noted:

```bash
dotnet restore RegistrationSystem.sln
dotnet build RegistrationSystem.sln
```

Run API:

```bash
dotnet run --project RegistrationWebAPI/RegistrationWebAPI.csproj
```

Run web app:

```bash
dotnet run --project RegistrationWebApp/RegistrationWebApp.csproj
```

Hot reload for web app:

```bash
dotnet watch run --project RegistrationWebApp/RegistrationWebApp.csproj
```

Run tests:

```bash
dotnet test Tests/Tests.csproj
```

Useful VS Code tasks already defined:

- `Build WebApp`
- `Build WebApi`
- `Build All Projects`
- `Dotnet Watch RegistrationWebApp`
- `Just Watch RegistrationWebApp`

## Architecture and ownership

- Keep shared domain contracts in `RegistrationShared` when both API and app need them.
- `RegistrationWebAPI/Program.cs` uses minimal APIs (not MVC controllers).
- DB entities and context live in `RegistrationWebAPI/Data`.
- Request/response contracts for API endpoints live in `RegistrationWebAPI/Models`.
- Blazor routable pages are in `RegistrationWebApp/Components/Pages`.
- Reusable UI components are in `RegistrationWebApp/Components/Layout/CustomLayout`.
- Web app API calling logic is in `RegistrationWebApp/Components/Utilities/WebApiUtility.cs`.

## Coding conventions and change style

- Follow existing naming and structure in each project.
- Keep changes minimal and scoped to the requested behavior.
- Reuse existing models/enums/services before introducing new abstractions.
- For Blazor forms, use built-in validation (`EditForm`, `DataAnnotationsValidator`, `ValidationMessage`) and avoid ad-hoc validation patterns.
- Avoid large formatting-only diffs.

## API and database rules

- Any entity/schema changes in `RegistrationWebAPI/Data` require a new EF Core migration.
- Commit both:
  - New migration file(s) under `RegistrationWebAPI/Migrations`
  - Updated `RegistrationDbContextModelSnapshot.cs`

### Migration command

From `RegistrationWebAPI`:

```bash
dotnet ef migrations add <MigrationName>
```

## Static content requirements in WebAPI

`verification.html` and `downloads.html` are required runtime assets and must remain published/copied with the API.

- Keep the `Content` entries in `RegistrationWebAPI.csproj` intact unless intentionally replacing this behavior.
- Be careful when editing startup/content-root logic in API because these templates are loaded at runtime.

## Testing guidance

- API integration tests use `WebApplicationFactory` and an in-memory SQLite connection.
- Test fixture sets auth/JWT/SMTP environment variables for API startup.
- `Smtp__ApiKey` must be available when running code paths that construct mail services.
- UI tests (`PlaywrightRegistrationWebAppTests`) start the app with `dotnet run --no-build`, so build first if binaries are stale.

Recommended stable test run when `dotnet watch` is active in Debug:

```bash
dotnet test Tests/Tests.csproj -c Release
```

## Security and configuration

- Do not hardcode credentials or secrets.
- Use configuration/environment variables for auth, JWT, SMTP, and URLs.
- Preserve JWT/auth checks when changing secured endpoints (`/health` and `/api/auth/token` are the intended anonymous endpoints).

## When implementing features or fixes

1. Identify the correct project boundary (`Shared`, `WebAPI`, `WebApp`, `Tests`).
2. Apply minimal code changes in the relevant layer(s).
3. Add/update tests where behavior changes.
4. Build affected projects.
5. Summarize changed files and behavior impact clearly.
