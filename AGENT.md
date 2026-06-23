# APSIM Registration System - Agent Guidance

This file provides detailed context for AI agents working on the APSIM Registration System codebase.

## Project Overview

The APSIM Registration System is a full-stack .NET solution for managing product registrations and downloads. It consists of:

- **RegistrationShared**: Shared enums and models (`net10.0`)
- **RegistrationWebAPI**: ASP.NET Core minimal API backend (`net10.0`)
- **RegistrationWebApp**: Blazor Server web application (`net10.0`)
- **Tests**: Automated test suite

**Target Framework**: .NET 10.0 (preview)
**Language**: C# 13
**Version Control**: Git (master branch)

## Technology Stack

### Backend
- **ASP.NET Core Minimal APIs** - Lightweight endpoint routing without controllers
- **Entity Framework Core 10.0.5** - ORM with SQLite database
- **JWT Bearer Authentication** - Token-based API security
- **Swagger/OpenAPI** - Interactive API documentation

### Frontend
- **Blazor Server** - Interactive server render mode components
- **Bootstrap 5** - CSS framework with responsive grid system
- **Razor Components** - Reusable `.razor` component files
- **Custom CSS** - Scoped styles via `.razor.css` files

### Database
- **SQLite** - File-based relational database
- **Migrations** - EF Core code-first migrations in `RegistrationWebAPI/Migrations/`

## Project Structure & Conventions

### Directory Organization

```
APSIM.RegistrationSystem/
├── RegistrationShared/
│   ├── Enums/              - Enumeration types shared across projects
│   └── Models/             - Domain models (User, Organisation)
│
├── RegistrationWebAPI/
│   ├── Data/               - EF Core DbContext and entity definitions
│   ├── Models/             - Request/response DTOs
│   ├── Utilities/          - Helper classes (MailUtility, etc.)
│   ├── Migrations/         - EF Core database migrations
│   ├── Program.cs          - API startup and configuration
│   └── appsettings*.json   - Configuration files
│
├── RegistrationWebApp/
│   ├── Components/
│   │   ├── Pages/          - Routable pages (8 pages total)
│   │   ├── Layout/         - Layout components (MainLayout, NavMenu, Footer, etc.)
│   │   ├── LayoutObjects/  - View models used by layouts
│   │   ├── Utilities/      - Service classes (WebApiUtility, etc.)
│   │   └── Classes/        - Domain models and enums
│   ├── Properties/         - launchSettings.json
│   ├── wwwroot/            - Static assets (CSS, JS, images)
│   ├── Program.cs          - App startup configuration
│   └── appsettings*.json   - Configuration files
│
├── Tests/
│   ├── TestRegistrationWebAPI.cs - Integration tests
│   ├── Utilities/          - Test helpers
│   └── Tests.csproj        - Test project
│
├── docker-compose.yml      - Multi-service Docker orchestration
├── RegistrationSystem.sln  - Visual Studio solution file
├── README.md               - Main project documentation
└── AGENT.md               - This file
```

### Naming Conventions

- **Enums**: `PascalCaseEnum.cs` (e.g., `UserLicenceStatus.cs`)
- **Models**: `PascalCaseModel.cs` (e.g., `User.cs`, `Organisation.cs`)
- **Entities**: `PascalCaseEntity.cs` (e.g., `UserEntity.cs`)
- **Components**: `PascalCase.razor` with optional scoped CSS (e.g., `ProductBox.razor`, `ProductBox.razor.css`)
- **Pages**: `PascalCase.razor` in `Components/Pages/` folder
- **CSS Classes**: `kebab-case` for CSS class names (Bootstrap convention)

## Build System & Tasks

### Available VS Code Tasks

| Task Name | Command | Purpose |
|-----------|---------|---------|
| Dotnet Watch RegistrationWebApp | `dotnet watch run --project RegistrationWebApp.csproj` | Hot-reload web app development |
| Just Watch RegistrationWebApp | Using `dotnet` task runner | Alternative hot-reload configuration |
| Build WebApp | `dotnet build RegistrationWebApp.csproj` | Build web app only |
| Build WebApi | `dotnet build RegistrationWebAPI.csproj` | Build API only |
| Build All Projects | `dotnet build` | Build entire solution |

### Manual Build Commands

```bash
# From repository root

# Restore dependencies
dotnet restore

# Build solution
dotnet build RegistrationSystem.sln

# Build specific project
dotnet build RegistrationWebApp/RegistrationWebApp.csproj
dotnet build RegistrationWebAPI/RegistrationWebAPI.csproj

# Run tests
dotnet test Tests/Tests.csproj

# Run web app with hot reload
dotnet watch run --project RegistrationWebApp/RegistrationWebApp.csproj

# Run API
dotnet run --project RegistrationWebAPI/RegistrationWebAPI.csproj
```

### Docker Commands

```bash
# Build images
docker build -f RegistrationWebAPI/Dockerfile -t apsim-registration-webapi .
docker build -f RegistrationWebApp/Dockerfile -t apsim-registration-webapp .

# Run with Docker Compose
docker compose up -d --build

# Stop services
docker compose down
```

## Important Project Details

### Pages & Routing

The RegistrationWebApp has 8 routable pages:

| Route | Component | Purpose | Key Features |
|-------|-----------|---------|--------------|
| `/` | Home.razor | Product overview | Card-based intro with icons |
| `/register` | Register.razor | Helps users decide between licence types | Summary table with buttons to select licence |
| `/special` | SpecialRegistration.razor | Special use registration | Organization details |
| `/download` | Download.razor | Download interface | OS-specific buttons, access control |
| `/validate` | Validate.razor | Validate emails and registers unregistered users | Email input |
| `/admin` | Admin.razor | Admin dashboard | Registration viewer, audit trail |
| `/error` | Error.razor | Error display | Error boundary component |
| (404) | NotFound.razor | Page not found | Status code re-execution |

### Bootstrap Classes Used Extensively

The application uses Bootstrap 5 throughout. Common patterns:

```html
<!-- Grid system -->
<div class="row row-cols-1 row-cols-md-3 g-4">
  <div class="col">Content</div>
</div>

<!-- Cards -->
<div class="card h-100 border-0 shadow-sm">
  <div class="card-body p-4">Content</div>
</div>

<!-- Buttons -->
<button class="btn btn-primary rounded-pill m-2">Click me</button>

<!-- Spacing utilities -->
<div class="mb-4 mt-4 p-3">Content with margins and padding</div>
```

### Scoped Styling Pattern

CSS files are co-located with Razor components and scoped to avoid conflicts:

```
Pages/
├── Home.razor
└── Home.razor.css  ← Scoped to Home.razor only
```

The CSS file should contain all styles specific to that component.

### Authentication & Security

- **JWT Tokens**: Generated via `POST /api/auth/token`
- **Bearer Authentication**: Include `Authorization: Bearer <token>` in API requests
- **Unauthenticated Endpoints**: Only `/health` and `/api/auth/token` don't require authentication
- **Token Format**: Uses HS256 algorithm (HMAC with SHA-256)

### Configuration

#### Environment Variables (RegistrationWebAPI)

```env
AUTH_USERNAME=admin
AUTH_PASSWORD=secure-password
JWT_ISSUER=APSIM.RegistrationSystem
JWT_AUDIENCE=APSIM.RegistrationSystem.Client
JWT_SIGNING_KEY=your-secret-key-minimum-32-characters
JWT_TOKEN_EXPIRY_MINUTES=60
SMTP_HOST=smtp.example.com
SMTP_PORT=587
SMTP__ApiKey=api-key-for-email
```

#### App URLs

- **Web App**: `https://localhost:5012` (development)
- **Web API**: `https://localhost:7276` (HTTPS), `http://localhost:5276` (HTTP)
- **Docker API**: `http://localhost:8088`
- **Docker App**: `http://localhost:8089`

### Database

- **Provider**: SQLite (`APSIMRegistrationSystemWebAPI.db`)
- **Location**: `RegistrationWebAPI/` root directory
- **Migrations**: Code-first via EF Core
- **Entities**:
  - `UserEntity` - Registered users
  - `OrganisationEntity` - Organizations
  - `DownloadAuditEntity` - Download tracking
  - `RegistrationDbContext` - DbContext

### Important Notes

1. **Build Artifacts**: The app builds successfully but may show file lock warnings if a previous instance is running. Stop the app before rebuilding.

2. **.NET Version**: Project targets `net10.0` (preview release). Ensure .NET SDK 10.0 is installed.

3. **Entity Framework Migrations**: When adding/modifying database entities:
   ```bash
   cd RegistrationWebAPI
   dotnet ef migrations add MigrationName
   dotnet ef database update
   ```
   See `update-ef-migration.bat` in the API project.

4. **Email Integration**: The API creates a `MailUtility` at startup (referenced in build notes). Ensure `Smtp__ApiKey` environment variable is set when running tests.

5. **API Documentation**: Swagger UI available at `https://localhost:7276/swagger/index.html` when running the API.

6. **Static Assets**: HTML templates (`verification.html`, `downloads.html`) are published to output. See `.csproj` for `CopyToPublishDirectory`.

## Common Tasks & Approaches

### Adding a New Page

1. Create `Components/Pages/YourPage.razor`
2. Add `@page "/your-route"` at the top
3. Create optional `Components/Pages/YourPage.razor.css` for scoped styles
4. Import necessary using statements from `_Imports.razor`
5. Build and test with `dotnet watch run`

### Adding a Reusable Component

1. Create in `Components/Layout/CustomLayout/YourComponent.razor`
2. Define `[Parameter]` properties for input
3. Create `.razor.css` file if styling needed
4. Use from pages with `<YourComponent Property="value" />`

### Modifying Home Page Cards

The Home page uses Bootstrap grid with scoped icon styles:

- Edit text in `Home.razor` (lines 14-46)
- Modify icons in `Home.razor.css` using `.intro-card-icon-*` classes
- Icons use SVG data URIs for self-contained styling

### Updating Database Schema

1. Modify entity in `RegistrationWebAPI/Data/`
2. Create migration: `dotnet ef migrations add DescriptiveName`
3. Review generated migration file
4. Apply: `dotnet ef database update`
5. Commit both the entity and migration files

### Running Tests

```bash
# Run all tests
dotnet test Tests/Tests.csproj

# Run specific test class
dotnet test Tests/Tests.csproj --filter TestClass

# Run with coverage (requires additional setup)
dotnet test Tests/Tests.csproj /p:CollectCoverage=true
```

**Note**: Tests may require `Smtp__ApiKey` environment variable. Set it before running tests.

## Code Patterns & Best Practices

### Razor Components

```razor
@using namespace.for.models
@namespace Your.Component.Namespace

<div class="card">
    @foreach (var item in Items)
    {
        <div class="card-body">
            @item.Name
        </div>
    }
</div>

@code {
    [Parameter]
    public required string Title { get; set; }

    [Parameter]
    public List<Item> Items { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        // Initialization logic
    }
}
```

### Service Classes

Use dependency injection:

```csharp
public class MyService
{
    private readonly IConfiguration _config;

    public MyService(IConfiguration config)
    {
        _config = config;
    }
}
```

### Minimal API Endpoints

```csharp
app.MapGet("/api/items", GetItems)
    .WithName("GetItems")
    .WithOpenApi()
    .RequireAuthorization();

async Task<IResult> GetItems(ILogger<Program> logger)
{
    // Implementation
}
```

## Git & Version Control

- **Main Branch**: `master`
- **Commit Convention**: Clear, descriptive messages ("Add user registration form", "Fix download button styling")
- **PR Process**: Create feature branch, make changes, open PR with summary
- **Testing**: Ensure `dotnet build` succeeds before committing

## Debugging Tips

1. **Razor Syntax Errors**: Build fails with clear error messages. Check line numbers.
2. **Missing Using Statements**: Check `_Imports.razor` for global using declarations.
3. **CSS Not Applying**: Verify `.razor.css` file name matches component exactly.
4. **API Connection Issues**: Check `appsettings.json` for correct API URL and port.
5. **Database Errors**: Verify SQLite file exists and migrations are applied.
6. **Hot Reload Issues**: Stop the app, rebuild, and restart with `dotnet watch run`.

## File Lock & Build Issues

If you encounter "file locked" warnings during build:

```bash
# Stop running processes
Stop-Process -Name "RegistrationWebApp" -Force

# Clean and rebuild
dotnet clean
dotnet build
```

## Resources & Documentation

- **Root README**: `README.md` - Project overview and quick start
- **RegistrationShared README**: `RegistrationShared/README.md` - Shared models/enums
- **API README**: `RegistrationWebAPI/README.md` - API endpoints and configuration
- **WebApp README**: `RegistrationWebApp/README.md` - Pages and components
- **Swagger UI**: Available at `https://localhost:7276/swagger/index.html` (when API running)

## Key Dependencies

- NuGet packages are declared in `.csproj` files
- Bootstrap is served from `wwwroot/lib/bootstrap/`
- No external CDNs required for core functionality
- JWT handling via built-in authentication middleware

---

**Last Updated**: June 23, 2026
**For**: AI agents assisting with development tasks
**Scope**: APSIM Registration System repository
