# RegistrationWebApp

A modern ASP.NET Core minimal Web API for managing general and special use registrations with JWT-based authentication and SQLite persistence.

## Overview

This API provides a complete registration management system supporting two registration types:

- **General Use Registrations**: Basic registrations requiring only contact information
- **Special Use Registrations**: Enhanced registrations requiring organization details and licensing information

All endpoints (except authentication and health checks) are protected with JWT bearer token authentication.

## Technology Stack

- **.NET 10.0** - Latest .NET runtime
- **ASP.NET Core Minimal APIs** - Lightweight API endpoints without controllers
- **Entity Framework Core 10.0.5** - ORM with SQLite provider
- **JWT Bearer Authentication** - Token-based security
- **Swagger/OpenAPI** - Interactive API documentation
- **SQLite** - File-based relational database

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Git

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/APSIMInitiative/APSIM.RegistrationAPIV2.git
   cd APSIM.RegistrationAPIV2
   ```

2. **Configure environment variables**

   Copy the example configuration:

   ```bash
   cp .env.example .env
   ```

   Then edit `.env` with your configuration:

   ```env
   AUTH_USERNAME=your-username
   AUTH_PASSWORD=your-secure-password
   JWT_ISSUER=APSIM.RegistrationAPIV2
   JWT_AUDIENCE=APSIM.RegistrationAPIV2.Client
   JWT_SIGNING_KEY=your-secret-key-minimum-32-characters-long
   JWT_TOKEN_EXPIRY_MINUTES=60
   ```

3. **Restore dependencies**

   ```bash
   dotnet restore
   ```

4. **Build the solution**

   ```bash
   dotnet build
   ```

## Running the API

### From Command Line

```bash
dotnet run --project APSIM.RegistrationAPIV2
```

The API will start on `https://localhost:7276` (HTTPS) and `http://localhost:5276` (HTTP).

### From Visual Studio

Press `F5` or select **Debug > Start Debugging**.

## API Documentation

Once the API is running, browse to:

- **Swagger UI**: https://localhost:7276/swagger/index.html
- **OpenAPI Spec**: https://localhost:7276/swagger/v1/swagger.json

## API Endpoints

### Authentication

#### 1. Create JWT Token

```text
POST /api/auth/token
Content-Type: application/json

{
  "username": "your-username",
  "password": "your-secure-password"
}
```

**Response (200 OK)**:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2026-03-25T15:30:00Z"
}
```

**Errors**:

- `401 Unauthorized` - Invalid credentials

---

### Health Check

#### 2. Get API Health

```text
GET /health
```

**Response (200 OK)**:

```json
{
  "status": "ok"
}
```

*No authentication required*

---

### Registrations

**All registration endpoints require JWT bearer authentication:**

```text
Authorization: Bearer <accessToken>
```

#### 3. List Registrations

```text
GET /api/registrations
```

**Query Parameters** (optional):

- `registrationType` - Filter by type: `GeneralUse` or `SpecialUse`
- `licenceStatus` - Filter by status (see Enums section)
- `contactEmail` - Filter by email address

**Example**:

```text
GET /api/registrations?registrationType=SpecialUse&licenceStatus=Active
```

**Response (200 OK)**:

```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "registrationType": "GeneralUse",
    "contactName": "John Doe",
    "contactEmail": "john@example.com",
    "applicationDate": "2026-03-20T10:30:00Z",
    "licenceStatus": "GeneralUse"
  }
]
```

---

#### 4. Get Registration by ID

```text
GET /api/registrations/{id}
```

**Response (200 OK)**:

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "registrationType": "GeneralUse",
  "contactName": "John Doe",
  "contactEmail": "john@example.com",
  "applicationDate": "2026-03-20T10:30:00Z",
  "licenceStatus": "GeneralUse"
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

- **Development**: `APSIMRegistrationV2_Dev.db`
- **Production**: `APSIMRegistrationV2.db`

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

4. **Consider Additional Security**
   - Implement rate limiting
   - Add request logging and monitoring
   - Enable CORS with appropriate origin restrictions

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

**"Database migration failed"**: Delete `APSIMRegistrationV2*.db` files to reset the database, then rebuild.

## Contributing

1. Create a feature branch: `git checkout -b feature/my-feature`
2. Commit changes: `git commit -am 'Add my feature'`
3. Push to branch: `git push origin feature/my-feature`
4. Open a Pull Request

## License

[Your License Here]

## Contact

For questions or support, contact the APSIM team in this repository by creating an issue.
