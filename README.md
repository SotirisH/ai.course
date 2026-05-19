# Config Service

A REST Web API Configuration Service built with .NET 10, PostgreSQL v16, and Entity Framework Core following Clean Architecture principles.

## Features

- **Application Management**: Create, read, update applications
- **Configuration Management**: Manage configurations with key-value pairs stored as JSONB
- **ULID Primary Keys**: Using ULID for globally unique, sortable identifiers
- **Clean Architecture**: Separation of concerns across Domain, Application, Infrastructure, and API layers
- **Input Validation**: FluentValidation for comprehensive request validation
- **Exception Handling**: Global exception handling middleware
- **Database Migrations**: EF Core migrations for schema management

## Tech Stack

- **.NET 10** (version 10.0.202)
- **PostgreSQL v16** with JSONB support
- **Entity Framework Core 10.0.0**
- **FluentValidation 11.11.0**
- **Serilog 4.2.0** for logging
- **xUnit 2.9.3** for testing
- **Swagger/OpenAPI** for API documentation

## Prerequisites

- .NET 10 SDK (version 10.0.202)
- PostgreSQL v16
- Docker (optional, for running PostgreSQL in a container)

## Getting Started

### 1. Set up PostgreSQL

**Option A: Using Docker**
```bash
docker run --name postgres-config -e POSTGRES_PASSWORD=yourpassword -p 5432:5432 -d postgres:16
```

**Option B: Install PostgreSQL locally**
Download from https://www.postgresql.org/download/

### 2. Configure environment variables

Copy the example environment file and update with your database credentials:
```bash
cp .env.example .env
```

Edit `.env` with your database connection string:
```
DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=configservice;Username=postgres;Password=yourpassword
ASPNETCORE_ENVIRONMENT=Development
LOGGING_LEVEL=Information
```

### 3. Restore dependencies

```bash
dotnet restore
```

### 4. Run database migrations

```bash
cd src/ConfigService.Api
dotnet ef database update --project ../ConfigService.Infrastructure
```

### 5. Run the application

```bash
dotnet run --project src/ConfigService.Api
```

The API will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Applications

- `POST /api/v1/applications` - Create a new application
- `GET /api/v1/applications` - Get all applications
- `GET /api/v1/applications/{id}` - Get application by ID
- `PUT /api/v1/applications/{id}` - Update an application

### Configurations

- `POST /api/v1/configurations` - Create a new configuration
- `GET /api/v1/configurations/{id}` - Get configuration by ID
- `PUT /api/v1/configurations/{id}` - Update a configuration

## Project Structure

```
config-service/
├── src/
│   ├── ConfigService.Api/          # Web API layer
│   ├── ConfigService.Application/  # Business logic layer
│   ├── ConfigService.Domain/       # Domain entities and exceptions
│   └── ConfigService.Infrastructure/ # Data access layer
├── .env                            # Environment variables (not in git)
├── .env.example                    # Example environment variables
├── .gitignore                      # Git ignore file
└── README.md                       # This file
```

## Database Schema

### Applications Table
- `id` (string, ULID, PK)
- `name` (string, unique)
- `comments` (string, nullable)

### Configurations Table
- `id` (string, ULID, PK)
- `application_id` (string, FK to applications)
- `name` (string)
- `comments` (string, nullable)
- `config` (jsonb)
- Unique constraint on (application_id, name)

## Development

### Build the solution
```bash
dotnet build
```

### Run tests
```bash
dotnet test
```

### Watch mode (auto-reload on changes)
```bash
dotnet watch --project src/ConfigService.Api
```

### Create a new migration
```bash
dotnet ef migrations add <MigrationName> --project src/ConfigService.Infrastructure --startup-project src/ConfigService.Api
```

### Rollback to specific migration
```bash
dotnet ef database update <MigrationName> --project src/ConfigService.Infrastructure --startup-project src/ConfigService.Api
```

## Future Enhancements

- Feature flags support (Module 3)
- Authentication and authorization
- Caching layer
- API versioning
- Health checks
- Docker containerization
- CI/CD pipeline

## License

This project is part of an AI course module.

