# RegistrationWebApp

Blazor web application for APSIM product registration workflows.

This project provides:

- A public registration experience for APSIM products.
- Guided licence-pathway selection for general and special use registration.
- An admin page for viewing sample registration records.

## Tech Stack

- .NET 10 (`net10.0`)
- ASP.NET Core Razor Components (Blazor Server, interactive server render mode)
- Bootstrap-based UI with custom reusable components

## Getting Started

### Prerequisites

- .NET SDK 10.0 (preview, matching `TargetFramework: net10.0`)

### Run Locally

From the repository root:

```bash
dotnet watch run --project RegistrationWebApp.csproj
```

Then open the URL shown in terminal output (typically `https://localhost:5012`).

### Build

```bash
dotnet build
```

## Application Routes

- `/` - Home page with APSIM product choices.
- `/register/{ProductId?}` - Registration page and licence decision flow.
  - Expected `ProductId` values include `nextgen` and `classic`.
- `/admin/` - Admin page showing example registrations.
- `/not-found` - Not found page used by status code re-execute middleware.

## Project Structure

Top-level layout:

- `Program.cs`: Application startup, middleware pipeline, and component mapping.
- `Components/Routes.razor`: Router configuration and default layout wiring.
- `Components/Pages/`: Routable pages (`Home`, `Register`, `Admin`, `Error`, `NotFound`).
- `Components/Layout/`: Main layout and navigation components.
- `Components/Layout/CustomLayout/`: Reusable UI building blocks (cards, modal, PDF viewer, product box).
- `Components/Classes/`: Domain models, enums, interfaces, and utility helpers.
- `LayoutObjects/`: View models used by layout/page components.
- `wwwroot/`: Static assets (CSS, images, and documents).

## Current Status

The registration flow UI and form validation are in place. Data handling is currently local/in-memory.

Known implementation notes:

- Registration submit currently writes data to console and toggles PDF visibility in the UI.
- Admin page currently displays seeded sample data rather than persisted registrations.
- Backend persistence, API integration, and email/approval workflows are not yet implemented.

## Development Notes

- In development, detailed errors are shown by default.
- In non-development environments, the app uses `/Error` and HSTS.
- Status code pages are configured to re-execute to `/not-found`.

## Contributing

1. Create a feature branch from `main`.
2. Make and test your changes locally.
3. Open a pull request with a clear summary and testing notes.

## License

Refer to APSIM Initiative licensing and policy documentation for usage terms.
