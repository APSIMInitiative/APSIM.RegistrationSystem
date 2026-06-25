# RegistrationWebAPI

A modern ASP.NET Core minimal Web API for managing APSIM registrations with JWT-based authentication and SQLite persistence.

## Overview

This API provides a complete registration management system supporting:

- **General Use Registrations**: Basic registrations with user contact information
- **Special Use Registrations**: Enhanced registrations with organization details and licensing
- **Download Tracking**: Audit trail for APSIM downloads and access events
- **JWT Authentication**: Secure token-based API access

All endpoints (except `/health` and `/api/auth/token`) require JWT bearer token authentication.

## Technology Stack

- **.NET 10.0** - Latest .NET runtime
- **ASP.NET Core Minimal APIs** - Lightweight API endpoints without controllers
- **Entity Framework Core 10.0.5** - ORM with SQLite provider
- **JWT Bearer Authentication** - Token-based security
- **Swagger/OpenAPI** - Interactive API documentation
- **SQLite** - File-based relational database

## Getting Started

### Prerequisites

- .NET SDK 10.0 (matching solution)
- Git

### Installation

From the repository root:

```bash
# Restore all dependencies
dotnet restore

# Build the solution
dotnet build
```

## Running the API

### From Command Line

```bash
dotnet run --project RegistrationWebAPI/RegistrationWebAPI.csproj
```

The API will start on `https://localhost:7276` (HTTPS) and `http://localhost:5276` (HTTP).

### Using VS Code Task

1. Open VS Code in the repository
2. Select **Terminal** > **Run Task...**
3. Choose **Build WebApi**

### Docker

```bash
docker build -f RegistrationWebAPI/Dockerfile -t apsim-registration-webapi .
docker run -p 7276:7276 apsim-registration-webapi
```

## Project Structure

### Hierarchy Diagram

```
RegistrationWebAPI/
├── Data/                           (Entity Framework Core)
│   ├── RegistrationDbContext.cs   - EF Core DbContext
│   ├── UserEntity.cs              - User database entity
│   ├── OrganisationEntity.cs      - Organisation database entity
│   └── DownloadAuditEntity.cs     - Download audit trail entity
│
├── Models/                         (Request/Response DTOs)
│   ├── AuthTokenRequest.cs        - Login request model
│   ├── AuthTokenResponse.cs       - JWT token response model
│   ├── RegistrationErrorResponse.cs - Error response wrapper
│   ├── DownloadEventRequest.cs    - Download tracking request
│   ├── DownloadAuditResponse.cs   - Download audit response
│   ├── MemberOrganisationResponse.cs - Organisation response
│   ├── BulkDeleteRegistrationsRequest.cs - Bulk delete request
│   └── RegistrationType.cs        - Registration type enum
│
├── Migrations/                     (EF Core Database Migrations)
│   ├── 20260526014802_InitialMigration.cs
│   └── 20260527223413_AddDownloadAudits.cs
│
├── Utilities/                      (Helper Classes)
│   └── MailUtility.cs             - Email utility functions
│
├── Program.cs                      (API Startup & Configuration)
├── appsettings.json               (Configuration)
├── appsettings.Development.json   (Development overrides)
├── Dockerfile                      (Container configuration)
└── RegistrationWebAPI.csproj      (Project file)
└── update-ef-migration.bat        (Database migration script)
```

## Database Migrations

When modifying any database entity classes in the `Data/` folder (UserEntity, OrganisationEntity, DownloadAuditEntity, etc.), you must generate a new Entity Framework Core migration.

### Running Migrations

Execute the batch file from the RegistrationWebAPI directory:

```bash
cd RegistrationWebAPI
update-ef-migration.bat
```

This script will:
1. Create a new migration file in the `Migrations/` folder
2. Update the database context snapshot

**Important**: Commit both the new migration file and any changes to the snapshot file to version control.

### Manual Alternative

If the batch file is unavailable, run:

```bash
dotnet ef migrations add <MigrationName>
```

## API Documentation

Once the API is running, browse to:

- **Swagger UI**: https://localhost:7276/swagger/index.html
- **OpenAPI Spec**: https://localhost:7276/swagger/v1/swagger.json

## API Endpoints

### Core Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/token` | Generate JWT authentication token | ❌ |
| GET | `/health` | API health check | ❌ |
| GET/POST | `/api/registrations` | List or create registrations | ✅ |
| GET | `/api/registrations/{id}` | Get registration by ID | ✅ |
| GET/POST | `/api/users` | User management | ✅ |
| GET/POST | `/api/organisations` | Organisation management | ✅ |
| POST | `/api/downloads/events` | Track download events | ✅ |
| GET | `/api/downloads/audits` | Get download audit trail | ✅ |

### Authentication

**Generate JWT Token**

```http
POST /api/auth/token
Content-Type: application/json

{
  "username": "admin",
  "password": "secure-password"
}
```

Response:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2026-06-23T16:30:00Z"
}
```

### Health Check

**Get API Status** (No authentication required)

```http
GET /health
```

Response:
```json
{
  "status": "ok"
}
```

### Registrations (Requires Authentication)

**All requests must include:**
```http
Authorization: Bearer <accessToken>
```

**List Registrations**

```http
GET /api/registrations?registrationType=GeneralUse&licenceStatus=Active
```

Response:
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "registrationType": "GeneralUse",
    "contactName": "John Doe",
    "contactEmail": "john@example.com",
    "applicationDate": "2026-03-20T10:30:00Z",
    "licenceStatus": "Active"
  }
]
```

**Get Registration by ID**

```http
GET /api/registrations/{id}
```
}
```

**Errors**:

- `404 Not Found` - Registration not found

---

#### 5. Create Registration

```text
POST /api/registrations
Content-Type: application/json

{
  "registrationType": "GeneralUse",
  "contactName": "Jane Smith",
  "contactEmail": "jane@example.com"
}
```

**General Use Registration** (minimal):

```json
{
  "registrationType": "GeneralUse",
  "contactName": "Jane Smith",
  "contactEmail": "jane@example.com"
}
```

**Special Use Registration** (required fields):

```json
{
  "registrationType": "SpecialUse",
  "contactName": "Jane Smith",
  "contactEmail": "jane@example.com",
  "organisationName": "ACME Corp",
  "organisationAddress": "123 Main St",
  "licencePathway": "TypeOne",
  "annualTurnover": "BelowTwoMillion"
}
```

**Response (201 Created)**:

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "registrationType": "GeneralUse",
  "contactName": "Jane Smith",
  "contactEmail": "jane@example.com",
  "applicationDate": "2026-03-25T12:00:00Z",
  "licenceStatus": "GeneralUse"
}
```

**Location Header**:

```text
Location: /api/registrations/550e8400-e29b-41d4-a716-446655440001
```

**Errors**:

- `400 Bad Request` - Validation failed

---

#### 6. Update Registration

```text
PUT /api/registrations/{id}
Content-Type: application/json

{
  "registrationType": "GeneralUse",
  "contactName": "Jane Smith Updated",
  "contactEmail": "jane-new@example.com"
}
```

**Response (200 OK)**:

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "registrationType": "GeneralUse",
  "contactName": "Jane Smith Updated",
  "contactEmail": "jane-new@example.com",
  "applicationDate": "2026-03-25T12:00:00Z",
  "licenceStatus": "GeneralUse"
}
```

**Errors**:

- `404 Not Found` - Registration not found
- `400 Bad Request` - Validation failed

---

#### 7. Delete Registration

```text
DELETE /api/registrations/{id}
```

**Response (204 No Content)**:

```text
(empty body)
```

**Errors**:

- `404 Not Found` - Registration not found

---

## Data Models

### Registration Types

#### GeneralUse

- `contactName` (required) - Name of the contact person
- `contactEmail` (required) - Email address of the contact
- Status auto-set to `GeneralUse`

#### SpecialUse

All of the above, plus:

- `organisationName` (required) - Organization name
- `organisationAddress` (required) - Organization address
- `organisationWebsite` (optional) - Organization website
- `contactPhone` (optional) - Contact phone number
- `licencePathway` (required) - `TypeOne` or `TypeTwo`
- `annualTurnover` (required) - See enum values below
- Status auto-set to `SpecialAwaitingReview`

### Enums

#### LicencePathway

- `TypeOne` - Modifications shared back
- `TypeTwo` - Modifications private

#### AnnualTurnover

- `BelowTwoMillion` - Less than $2M AUD
- `TwoToFortyMillion` - $2M - $40M AUD
- `AboveFortyMillion` - Over $40M AUD

#### LicenceStatus

- `None` - No licence
- `GeneralUse` - General use licence
- `SpecialAwaitingReview` - Special use pending review
- `SpecialProvisional` - Special use provisional
- `SpecialInvoiced` - Special use invoiced
- `SpecialActive` - Special use active
- `SpecialDeclined` - Special use declined
- `Cancelled` - Registration cancelled
- `Expired` - Registration expired

## Configuration

### Environment Variables (.env file)

| Variable | Purpose | Example |
| ---------- | --------- | --------- |
| `AUTH_USERNAME` | API authentication username | `your-username` |
| `AUTH_PASSWORD` | API authentication password | `your-secure-password` |
| `JWT_ISSUER` | JWT issuer claim | `APSIM.RegistrationAPIV2` |
| `JWT_AUDIENCE` | JWT audience claim | `APSIM.RegistrationAPIV2.Client` |
| `JWT_SIGNING_KEY` | Secret key for signing JWT tokens (min 32 chars) | `your-secret-key-...` |
| `JWT_TOKEN_EXPIRY_MINUTES` | Token expiration time in minutes | `60` |

### Database

The API uses SQLite for persistence. The database file is created automatically on first run:

- **Development**: `APSIMRegistrationSystemWebAPI.db`
- **Production**: `APSIMRegistrationSystemWebAPI.db`

Database schema is managed with Entity Framework Core migrations. All migrations are applied automatically on startup.

## Development

### Project Structure

```text
APSIM.RegistrationAPIV2/
├── Data/
│   ├── RegistrationDbContext.cs       # EF Core DbContext
│   └── RegistrationEntity.cs          # Database entity model
├── Models/
│   ├── RegistrationUpsertRequest.cs   # Request DTO
│   ├── RegistrationResponse.cs        # Response DTO
│   ├── RegistrationType.cs            # Local enum
│   ├── AuthTokenRequest.cs            # Auth request DTO
│   └── AuthTokenResponse.cs           # Auth response DTO
├── Services/
│   ├── RegistrationMapping.cs         # Entity-to-DTO mappers
│   └── RegistrationValidation.cs      # Business logic validation
├── Migrations/
│   └── [EF Core migrations]
├── Program.cs                          # API startup configuration
└── appsettings*.json                   # Configuration files

APSIM.Registration.Contracts/
├── Enums/
│   ├── LicenceStatus.cs
│   ├── LicencePathway.cs
│   └── AnnualTurnover.cs
├── Interfaces/
│   └── IRegistration.cs
└── Models/
    ├── GeneralUseRegistration.cs
    └── SpecialUseRegistration.cs
```

### Building

```bash
# Clean build
dotnet clean
dotnet build

# Release build
dotnet build -c Release

# Restore NuGet packages
dotnet restore
```

### Running Tests

```bash
dotnet test
```

(Tests can be added to the `Tests/` folder)

## Security Considerations

### JWT Authentication

- Tokens expire after the configured `JWT_TOKEN_EXPIRY_MINUTES` (default: 60 minutes)
- Tokens are signed with `HS256` using a symmetric key
- Token validation includes issuer, audience, and signing key verification

### For Production Deployment

1. **Rotate Credentials**
   - Replace `AUTH_PASSWORD` with a strong password
   - Replace `JWT_SIGNING_KEY` with a random 32+ character string

2. **Use Secrets Manager**
   - Store credentials in Azure Key Vault, AWS Secrets Manager, or similar
   - Never commit `.env` files to version control

3. **HTTPS Only**
   - Ensure HTTPS is enforced in production
   - Update `AllowedHosts` in `appsettings.json` to specific domains


## Troubleshooting

### Build Errors

**"dotenv.net not found"**: Run `dotnet restore` to restore all NuGet packages.

**"SQLite database locked"**: Stop any running instances and try again. Kill the process if necessary:

```bash
# PowerShell
Stop-Process -Name dotnet -Force
```

### Runtime Issues

**"Auth:Password is not configured"**: Ensure the `.env` file exists and contains `AUTH_PASSWORD`.

**"Jwt:SigningKey is not configured"**: Check that `.env` includes `JWT_SIGNING_KEY` with at least 32 characters.

**"Database migration failed"**: Delete `APSIMRegistrationSystemWebAPI.db` files to reset the database, then rebuild.

## Contributing

1. Create a feature branch: `git checkout -b feature/my-feature`
2. Commit changes: `git commit -am 'Add my feature'`
3. Push to branch: `git push origin feature/my-feature`
4. Open a Pull Request


## Contact

For questions or support, contact the APSIM team in this repository by creating an issue.
