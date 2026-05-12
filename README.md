# APSIM Registration System

## Overview

This repository contains the APSIM registration platform, split across a shared library, a Web API, a Blazor web application, and tests.

## Solution Structure

- **[RegistrationShared](./RegistrationShared/README.md)**: Shared enums and models used by both API and web app.
- **[RegistrationWebAPI](./RegistrationWebAPI/README.md)**: ASP.NET Core minimal API for registration data and related workflows.
- **[RegistrationWebApp](./RegistrationWebApp/README.md)**: Blazor Server UI for registration and admin workflows.
- **[Tests](./Tests)**: Automated tests for API behavior and integration points.

## Quick Start

### Prerequisites

- .NET SDK 10.0 (matching project target framework `net10.0`)

### Restore Dependencies

From the repository root:

```bash
dotnet restore RegistrationSystem.sln
```

### Build

```bash
dotnet build RegistrationSystem.sln
```

### Run the Web API

```bash
dotnet run --project RegistrationWebAPI/RegistrationWebAPI.csproj
```

### Run the Web App (Standard)

Use this when you want a normal app run without file watching:

```bash
dotnet run --project RegistrationWebApp/RegistrationWebApp.csproj
```

### Run the Web App (Watch / Hot Reload)

Use watch mode during UI development so changes are picked up automatically and hot reload is applied when possible:

```bash
dotnet watch run --project RegistrationWebApp/RegistrationWebApp.csproj
```

### Run Watch Mode from VS Code Task Menu

You can start the same watch workflow from the VS Code terminal menu:

1. Open VS Code in this repository.
2. Select **Terminal** from the top menu.
3. Choose **Run Task...**.
4. Select **Just Watch RegistrationWebApp**.

This starts the configured task that runs the web app in watch mode from the `RegistrationWebApp` project directory.

### Run Tests

```bash
dotnet test Tests/Tests.csproj
```

## Project Documentation

For project-specific configuration, troubleshooting, and development details, see each project README linked above.

## Contributing

1. Create a feature branch from `master`.
2. Make and validate your changes locally.
3. Open a pull request with a summary of what changed and why.
