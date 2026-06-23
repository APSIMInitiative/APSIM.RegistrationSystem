# RegistrationWebApp

Blazor web application for APSIM product registration workflows.

This project provides:

- A public registration experience for APSIM products.
- Guided licence-pathway selection for general and special use registration.
- An admin page for viewing sample registration records.

## Tech Stack

- **.NET 10** (`net10.0`)
- **ASP.NET Core Razor Components** (Blazor Server with interactive server render mode)
- **Bootstrap 5** - Responsive CSS framework
- **Entity Framework Core 10** - Database access (via WebAPI)
- **Custom Components** - Reusable Razor components

## Getting Started

### Prerequisites

- .NET SDK 10.0 (preview, matching `TargetFramework: net10.0`)

### Run Locally

From the repository root:

```bash
cd RegistrationWebApp
dotnet watch run
```

Then open the URL shown in terminal output (typically `https://localhost:5012`).

### Using VS Code Task

1. Open VS Code in the repository
2. Select **Terminal** > **Run Task...**
3. Choose **Just Watch RegistrationWebApp** for hot reload during development

### Build

```bash
cd RegistrationWebApp
dotnet build
```

### Docker

```bash
docker build -f RegistrationWebApp/Dockerfile -t apsim-registration-webapp .
docker run -p 8089:8089 apsim-registration-webapp
```

## Application Routes & Pages

### Page Structure

| Route | Page Component | Purpose |
|-------|---|----------|
| `/` | Home.razor | Product selection and introduction |
| `/register/{productId?}` | Register.razor | General use registration form |
| `/special` | SpecialRegistration.razor | Special use license registration |
| `/download` | Download.razor | Download APSIM with access control |
| `/validate` | Validate.razor | Validate and download APSIM Classic |
| `/admin` | Admin.razor | Admin dashboard and registration viewer |
| `/error` | Error.razor | Error display page |
| (404) | NotFound.razor | Page not found handler |

## Project Structure

### Hierarchy Diagram

```
RegistrationWebApp/
├── Components/
│   ├── Pages/                      (Routable Pages)
│   │   ├── Home.razor              - Product overview with intro cards
│   │   ├── Register.razor          - General use registration form
│   │   ├── SpecialRegistration.razor - Special use registration form
│   │   ├── Download.razor          - Download interface
│   │   ├── Validate.razor          - APSIM Classic validation
│   │   ├── Admin.razor             - Admin dashboard
│   │   ├── Error.razor             - Error page
│   │   └── NotFound.razor          - 404 page
│   │
│   ├── Layout/                     (Layout Components)
│   │   ├── MainLayout.razor        - Primary layout wrapper
│   │   ├── MainLayout.razor.css    - Layout styles
│   │   ├── NavMenu.razor           - Navigation menu
│   │   ├── NavMenu.razor.css       - Nav menu styles
│   │   ├── Footer.razor            - Footer component
│   │   ├── Footer.razor.css        - Footer styles
│   │   ├── ReconnectModal.razor    - Connection loss modal
│   │   ├── ReconnectModal.razor.css - Modal styles
│   │   ├── ReconnectModal.razor.js - Modal interactivity
│   │   │
│   │   └── CustomLayout/           (Custom Layout Components)
│   │       ├── Card.razor          - Basic card component
│   │       ├── Card.razor.css
│   │       ├── ProductBox.razor    - Product display card
│   │       ├── ProductBox.razor.css
│   │       ├── DecisionCard.razor  - Decision UI card
│   │       ├── DecisionCard.razor.css
│   │       ├── DotPointCard.razor  - Bullet point card
│   │       ├── DotPointCard.razor.css
│   │       ├── CustomModal.razor   - Custom modal dialog
│   │       ├── PdfViewer.razor     - PDF display component
│   │       ├── PdfViewer.razor.css
│   │       └── BootstrapHelpers/   (Bootstrap utility components)
│   │
│   ├── LayoutObjects/              (Page-Level Objects)
│   │   └── Product.cs              - Product model for display
│   │
│   ├── Utilities/                  (Service Classes)
│   │   ├── WebApiUtility.cs        - API client wrapper
│   │   ├── APSIMBuildsAPIUtility.cs - APSIM builds API integration
│   │   ├── DownloadAccessState.cs  - Download state management
│   │   └── Models/                 (Utility Models)
│   │       ├── UserResponseModel.cs
│   │       ├── OrganisationResponseModel.cs
│   │       ├── Login.cs
│   │       ├── DownloadTokenValidationResponse.cs
│   │       ├── DownloadAuditResponse.cs
│   │       ├── DownloadEventRequest.cs
│   │       ├── DownloadCsvExportResult.cs
│   │       ├── APSIMNextGenDownloadInfo.cs
│   │       └── APSIMClassicDownloadInfo.cs
│   │
│   ├── Classes/                    (Domain Classes)
│   │   ├── AlertType.cs            - Alert type enumerations
│   │   ├── EmailModel.cs           - Email request model
│   │   └── Utilities/              (Additional utilities)
│   │
│   ├── App.razor                   - Root component (error boundary)
│   ├── Routes.razor                - Route definitions
│   └── _Imports.razor              - Global using statements
│
├── Properties/
│   └── launchSettings.json         - Launch configuration
│
├── wwwroot/                        (Static Assets)
│   ├── css/                        - CSS files
│   ├── js/                         - JavaScript files
│   ├── Images/                     - Product images
│   ├── lib/                        - Third-party libraries (Bootstrap, etc.)
│   └── favicon.png
│
├── Program.cs                      - App startup configuration
├── appsettings.json                - Configuration
├── appsettings.Development.json    - Development overrides
├── Dockerfile                      - Container configuration
└── RegistrationWebApp.csproj      - Project file
```

## Component Overview

### Pages

**Home.razor** (`/`)
- Introduction to APSIM Next Generation
- Product selection cards with icons
- Links to registration and legacy download

**Register.razor** (`/register/{productId?}`)
- General use registration form
- License pathway decision workflow
- Form validation and submission

**SpecialRegistration.razor** (`/special`)
- Special use license registration
- Organization details collection
- Annual turnover selection

**Download.razor** (`/download`)
- APSIM download interface
- OS-specific download buttons (Windows, macOS, Linux)
- Access control and audit tracking

**Validate.razor** (`/validate`)
- Classic APSIM validation page
- Legacy download access
- Token verification

**Admin.razor** (`/admin`)
- Admin dashboard
- View registered users and organizations
- Download audit trail

### Reusable Components

**ProductBox.razor** - Displays product information card with buttons
**Card.razor** - Generic card container component
**DecisionCard.razor** - Interactive decision selection card
**DotPointCard.razor** - Card with bullet-point content
**CustomModal.razor** - Modal dialog wrapper
**PdfViewer.razor** - PDF document display component

### Layout

**MainLayout.razor** - Primary layout with sidebar navigation
**NavMenu.razor** - Collapsible navigation menu
**Footer.razor** - Application footer
**ReconnectModal.razor** - Connection loss indicator

### Services

**WebApiUtility** - HTTP client for communicating with RegistrationWebAPI
**APSIMBuildsAPIUtility** - Integration with APSIM builds information
**DownloadAccessState** - Manages download access state and validation

## Configuration

The app connects to the RegistrationWebAPI for:
- User registration
- Organization data
- Download tracking
- Authentication

Configure the API base URL in `appsettings.json`.

## Development Notes

- In development, detailed errors are shown by default
- Hot reload is supported via `dotnet watch`
- Blazor Server uses interactive server render mode for real-time UI updates
- Status code pages are configured with re-execution middleware

## Contributing

1. Create a feature branch from `master`
2. Make and test your changes locally using `dotnet watch run`
3. Ensure the page compiles without errors
4. Open a pull request with a clear summary of changes

## Support

For issues or questions, contact the APSIM team.
