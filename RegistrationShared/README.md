# RegistrationShared

## Overview

RegistrationShared is a shared library for the APSIM Registration System, providing common enums and models used across both the API and web application.

## Purpose

This project contains reusable code that defines the core domain models and enumeration types for registration functionality within the APSIM ecosystem. It eliminates duplication and ensures consistency between the backend API and frontend application.

## Technology Stack

- **.NET 10.0** - Target framework matching the rest of the solution
- **C# 13** - Modern language features

## Project Structure

### Hierarchy Diagram

```
RegistrationShared/
├── Enums/                           (Enumeration Types)
│   ├── AnnualTurnover.cs           - Annual turnover brackets
│   ├── LicencePathway.cs           - Licence decision pathways
│   ├── OrganisationLicenceStatus.cs - Organisation licence states
│   └── UserLicenceStatus.cs        - User licence states
│
└── Models/                          (Domain Models)
    ├── User.cs                     - User entity model
    └── Organisation.cs             - Organisation entity model
```

## Key Components

### Enums

**AnnualTurnover.cs**
- Defines annual turnover brackets for organization classification
- Used in special use registration workflows

**LicencePathway.cs**
- Defines the decision pathways for licence selection
- Guides users through general vs. special use registration

**OrganisationLicenceStatus.cs**
- Tracks the licensing state of registered organisations
- States include: Active, Expired, Suspended, Pending

**UserLicenceStatus.cs**
- Tracks the licensing state of individual users
- States include: Active, Expired, Suspended, Pending

### Models

**User.cs**
- Represents a registered user in the APSIM ecosystem
- Properties: Id, Name, Email, LicenceStatus, etc.
- Referenced by RegistrationWebAPI and RegistrationWebApp

**Organisation.cs**
- Represents a registered organisation for special use licensing
- Properties: Id, Name, AnnualTurnover, LicenceStatus, etc.
- Supports multi-user organizations

## Usage

Reference this project in other APSIM registration system components:

```csharp
using RegistrationShared.Models;
using RegistrationShared.Enums;

var user = new User { /* ... */ };
var status = user.LicenceStatus; // UserLicenceStatus enum
```

## Building

From the repository root:

```bash
dotnet build RegistrationShared/RegistrationShared.csproj
```

## Dependencies

- No external NuGet dependencies (uses only .NET standard libraries)

## Contributing

When modifying shared models or enums:

1. Ensure changes are backward compatible where possible
2. Update both RegistrationWebAPI and RegistrationWebApp to use new types
3. Run tests to verify no breaking changes

## Support

For issues or questions, contact the APSIM team.
