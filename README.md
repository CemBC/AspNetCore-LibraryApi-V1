# Library Management System API

A production-ready Library Management REST API built with **ASP.NET Core and .NET 10**.

The project demonstrates a complete backend development workflow including authentication, authorization, business rules, automated testing, Docker containerization, cloud deployment, database hosting, health checks, and CI/CD.

The API is deployed to **Microsoft Azure** and uses **Azure SQL Database** in production.

---

# Live Deployment

The production API is hosted on **Microsoft Azure App Service** and uses **Azure SQL Database**.

### API

[https://libraryapplication-cb-gdfwakbcarfqfuda.italynorth-01.azurewebsites.net](https://libraryapplication-cb-gdfwakbcarfqfuda.italynorth-01.azurewebsites.net)

### Swagger UI

[https://libraryapplication-cb-gdfwakbcarfqfuda.italynorth-01.azurewebsites.net/swagger](https://libraryapplication-cb-gdfwakbcarfqfuda.italynorth-01.azurewebsites.net/swagger)

Swagger provides an interactive interface for exploring and testing the API endpoints.

### Scalar API Reference

[https://libraryapplication-cb-gdfwakbcarfqfuda.italynorth-01.azurewebsites.net/scalar/v1](https://libraryapplication-cb-gdfwakbcarfqfuda.italynorth-01.azurewebsites.net/scalar/v1)

Scalar provides a modern API reference interface for browsing the available endpoints and schemas.

### Health Check

[https://libraryapplication-cb-gdfwakbcarfqfuda.italynorth-01.azurewebsites.net/health](https://libraryapplication-cb-gdfwakbcarfqfuda.italynorth-01.azurewebsites.net/health)

A healthy deployment returns:

```text
Healthy
```

The health check also verifies connectivity to the production database.

---

# Technologies

## Backend

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- LINQ
- RESTful API Design

## Authentication & Authorization

- JWT Bearer Authentication
- Refresh Tokens
- Refresh Token Rotation
- Hashed Refresh Tokens
- Role-Based Authorization
- ASP.NET Core PasswordHasher

## Application Infrastructure

- FluentValidation
- AutoMapper
- Custom Exception Middleware
- Structured Logging
- Health Checks

## Testing

- xUnit
- WebApplicationFactory
- EF Core InMemory
- Unit Tests
- Integration Tests

## DevOps & Deployment

- Docker
- Docker Compose
- GitHub Actions
- OpenID Connect (OIDC)
- Azure App Service
- Azure SQL Database

## API Documentation

- Swagger / OpenAPI
- Scalar API Reference

---

# Repository Structure

```text
AspNetCore-LibraryApi-V1/
│
├── .github/
│   └── workflows/
│       └── main_libraryapplication-cb.yml
│
├── LibraryApi/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Exceptions/
│   ├── Helpers/
│   ├── Mappings/
│   ├── Middleware/
│   ├── Migrations/
│   ├── Models/
│   ├── Services/
│   ├── Validators/
│   ├── Dockerfile
│   ├── docker-compose.yaml
│   ├── LibraryApi.csproj
│   └── Program.cs
│
├── LibraryApi.Tests/
│   ├── Integration/
│   ├── Unit/
│   ├── Helpers/
│   └── LibraryApi.Tests.csproj
│
├── LibraryApi.slnx
└── README.md
```

The repository contains both the main API and its automated test project.

---

# Authentication

The API supports:

- User registration
- Login
- JWT access tokens
- Refresh tokens
- Refresh token rotation
- Hashed refresh token storage
- Logout
- Current authenticated user endpoint
- Role-based authorization

Available roles:

```text
Admin
Member
```

A registered user receives the `Member` role by default.

---

# Book Management

Admins can:

- Create books
- Update books
- Delete books
- View books

Members can:

- View books
- Search books
- Filter books by status
- Sort books
- Use pagination

Book statuses:

```text
Available
Requested
Loaned
```

---

# Member Management

Admins can:

- View members
- Update members
- Delete members
- View member loan history
- Search members
- Sort members
- Use pagination

Each registered member is connected to a `User` using a one-to-one relationship.

---

# Loan Management

The API supports:

- Manual loan creation by admins
- Loan creation from approved requests
- Book returns
- Active loan monitoring
- Overdue loan monitoring
- Member-specific loan history
- Searching
- Filtering
- Sorting
- Pagination

Loan statuses:

```text
Active
Returned
Overdue
```

---

# Loan Requests

Members can request available books.

Workflow:

```text
Member
   ↓
Creates Loan Request
   ↓
Book → Requested
   ↓
Admin Approves
   ↓
Loan → Active
   ↓
Book → Loaned
```

If the request is rejected:

```text
Loan Request → Rejected
Book → Available
```

Request statuses:

```text
Pending
Approved
Rejected
```

---

# Loan Extension Requests

Members can request an extension for active loans.

If the request is approved:

```text
DueDate = DueDate + 7 days
```

Extension statuses:

```text
Pending
Approved
Rejected
```

---

# Business Rules

The application includes domain-level business constraints rather than only CRUD operations.

## Maximum Active Loans

A member can have at most:

```text
3 unreturned loans
```

This rule is validated when:

- A member creates a loan request
- An admin approves a loan request
- An admin manually creates a loan

---

## Overdue Loan Restriction

Members with overdue loans cannot receive or request another book.

Overdue detection is based on the actual loan state:

```csharp
loan.ReturnDate == null &&
loan.DueDate < DateTime.UtcNow
```

This prevents stale status values from bypassing the business rule.

---

## Maximum Extensions

Each loan can have at most:

```text
2 approved extensions
```

---

## Overdue Extension Restriction

A loan cannot be extended after its due date has passed.

---

# Pagination, Search, Filtering and Sorting

Collection endpoints support query parameters.

Example:

```http
GET /api/Books?page=1&pageSize=10&search=Dune&status=1&sortBy=title&descending=false
```

Paged responses follow this structure:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalCount": 0,
  "totalPages": 0
}
```

Pagination is implemented for:

```text
Books
Members
Loans
Pending Loan Requests
Pending Loan Extension Requests
Active Loans
Overdue Loans
```

---

# Authorization

The API uses JWT Bearer authentication.

## Member Operations

Members can:

- View books
- View their own loans
- Create loan requests
- View their own pending loan requests
- Create loan extension requests
- View their own pending extension requests

## Admin Operations

Admins can:

- Manage books
- Manage members
- View all loans
- Create loans manually
- Return books
- View active loans
- View overdue loans
- Approve or reject loan requests
- Approve or reject extension requests
- View administrative statistics

---

# Admin Statistics

Admins can access:

```http
GET /api/Admin/statistics
```

Statistics include:

```text
Total Books
Available Books
Total Members
Active Loans
Overdue Loans
Pending Loan Requests
Pending Extension Requests
```

---

# Validation

Request validation is implemented using **FluentValidation**.

Validation is applied to DTOs before business logic is executed.

Examples include:

- Required fields
- Maximum string lengths
- Identifier validation
- Pagination validation
- Request-specific rules

Invalid requests return appropriate `400 Bad Request` responses.

---

# DTO Mapping

The application uses **AutoMapper** to map between:

```text
Entities
   ↕
DTOs
   ↕
API Responses
```

This prevents database entities from being directly exposed by API endpoints.

---

# Global Exception Handling

The API uses custom exception middleware.

Typical mappings:

| Exception | HTTP Status |
|---|---:|
| `BadRequestException` | 400 |
| `UnauthorizedException` | 401 |
| `NotFoundException` | 404 |
| Unexpected Exception | 500 |

Unexpected errors are logged using `ILogger`.

---

# Logging

Structured logging is used for important application events.

Examples include:

```text
Register
Login
Logout
Refresh Token
Create / Update / Delete Book
Create / Update / Delete Member
Create / Return Loan
Approve / Reject Loan Request
Approve / Reject Extension Request
Unexpected Application Errors
```

Sensitive values are never intentionally logged.

Examples:

```text
Passwords
Password hashes
JWT secrets
Access tokens
Refresh tokens
```

---

# Automated Testing

The repository contains a separate test project:

```text
LibraryApi.Tests
```

The current test suite contains:

```text
Total:   113
Passed:  112
Skipped: 1
Failed:  0
```

The suite contains both **unit tests** and **integration tests**.

Covered areas include:

- Authentication
- JWT generation
- Refresh token rotation
- Authorization
- Books
- Members
- Loans
- Loan requests
- Loan extensions
- Business constraints
- Pagination
- Search
- Filtering
- Sorting
- Validation
- Exception middleware
- Admin statistics
- Complete API workflows

---

## Integration Testing

Integration tests use:

```text
WebApplicationFactory
Entity Framework Core InMemory
```

The integration environment uses an isolated in-memory database.

Therefore automated tests do not modify the development or production SQL Server database.

---

## Run Tests

From the repository root:

```bash
dotnet test ./LibraryApi.slnx
```

Release configuration:

```bash
dotnet test ./LibraryApi.slnx --configuration Release
```

---

# CI/CD

The repository uses **GitHub Actions** for continuous integration and continuous deployment.

Every push to:

```text
main
```

triggers the deployment workflow.

Pipeline:

```text
Push to main
      ↓
Checkout Repository
      ↓
Restore Dependencies
      ↓
Build Solution
      ↓
Run 113 Automated Tests
      ↓
Publish API
      ↓
Create Deployment Artifact
      ↓
Authenticate to Azure using OIDC
      ↓
Deploy to Azure App Service
```

If the build or any automated test fails:

```text
Deployment is stopped.
```

This prevents broken application code from being automatically deployed to production.

---

# Azure Authentication for CI/CD

GitHub Actions authenticates to Azure using:

```text
OpenID Connect (OIDC)
```

A user-assigned managed identity is used instead of storing a long-lived Azure password or deployment credential in the repository.

The deployment identity only has access to the required Azure App Service resource.

---

# Azure Deployment

Production architecture:

```text
Client / Future Frontend
          │
          │ HTTPS
          ▼
┌─────────────────────────┐
│ Azure App Service       │
│ LibraryApi              │
│ ASP.NET Core / .NET 10  │
└────────────┬────────────┘
             │
             │ SQL Connection
             ▼
┌─────────────────────────┐
│ Azure SQL Database      │
│ LibraryDb               │
└─────────────────────────┘
```

Production configuration such as JWT secrets and database credentials is stored using Azure App Service environment configuration.

Secrets are not committed to the repository.

---

# Configuration and Secrets

Different environments use different configuration sources.

```text
Local Development
→ .NET User Secrets

Docker
→ .env

Azure Production
→ Azure App Service Environment Variables
```

Sensitive values such as:

```text
JWT keys
Database passwords
Connection strings containing credentials
```

must never be committed to Git.

---

# Docker

The API and SQL Server can run together using Docker Compose.

Docker architecture:

```text
Browser / Client
       │
       │ localhost:8080
       ▼
┌─────────────────────┐
│ LibraryApi          │
│ ASP.NET Core        │
│ Container           │
└─────────┬───────────┘
          │
          │ Docker Network
          ▼
┌─────────────────────┐
│ SQL Server 2022     │
│ Container           │
└─────────┬───────────┘
          │
          ▼
     Docker Volume
```

---

# Docker Requirements

Install:

- Docker Desktop
- WSL 2 on Windows

Verify Docker:

```bash
docker --version
```

Test the Docker engine:

```bash
docker run hello-world
```

---

# Docker Environment Variables

Docker configuration uses:

```text
.env
```

A safe example configuration is provided through:

```text
.env.example
```

Example:

```env
MSSQL_SA_PASSWORD=YourStrongPasswordHere123!

JWT_KEY=YourJwtSecretKeyHere
JWT_ISSUER=LibraryApi
JWT_AUDIENCE=LibraryApiClient
```

The real `.env` file is excluded from Git.

Never commit real passwords or JWT secrets.

---

# Start with Docker Compose

Navigate to the API project directory:

```bash
cd LibraryApi
```

Build and start the API and SQL Server:

```bash
docker compose up -d --build
```

If the application image already exists:

```bash
docker compose up -d
```

Check running services:

```bash
docker compose ps
```

---

# Stop Docker Services

```bash
docker compose down
```

This removes the running API and SQL Server containers while keeping the database volume.

Do not normally use:

```bash
docker compose down -v
```

because `-v` also deletes the persisted SQL Server volume.

---

# Docker Logs

API logs:

```bash
docker compose logs api
```

SQL Server logs:

```bash
docker compose logs sqlserver
```

Live logs:

```bash
docker compose logs -f
```

---

# Docker Resource Usage

View resource usage:

```bash
docker stats
```

View disk usage:

```bash
docker system df
```

View Docker volumes:

```bash
docker volume ls
```

On Windows, the WSL backend can be completely stopped with:

```powershell
wsl --shutdown
```

Docker Desktop will restart WSL when Docker is launched again.

---

# Docker SQL Server

Docker exposes SQL Server through:

```text
localhost,1433
```

Example SSMS configuration:

```text
Server: localhost,1433
Authentication: SQL Server Authentication
Login: sa
Password: MSSQL_SA_PASSWORD value
```

If necessary:

```text
Trust Server Certificate = True
```

Inside the Docker network, the API connects using the Compose service name:

```text
sqlserver
```

rather than:

```text
localhost
```

---

# Database Persistence

Docker SQL Server data is stored in a persistent volume.

Therefore:

```bash
docker compose down
```

does not remove data such as:

```text
Users
Members
Books
Loans
Loan Requests
Extension Requests
```

The Docker volume consumes disk space but does not consume CPU or RAM while containers are stopped.

---

# Entity Framework Core Migrations

Database schema changes are managed using EF Core migrations.

Create a migration:

```bash
dotnet ef migrations add MigrationName --project ./LibraryApi/LibraryApi.csproj
```

Apply migrations:

```bash
dotnet ef database update --project ./LibraryApi/LibraryApi.csproj
```

For Docker SQL Server, the connection string can temporarily be overridden:

```powershell
$env:ConnectionStrings__LibraryDb="Server=localhost,1433;Database=LibraryDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=True"
```

Then:

```powershell
dotnet ef database update --project ./LibraryApi/LibraryApi.csproj
```

Remove the temporary variable afterwards:

```powershell
Remove-Item Env:ConnectionStrings__LibraryDb
```

---

# Health Checks

The API exposes:

```http
GET /health
```

Local Docker example:

```text
http://localhost:8080/health
```

Production example:

```text
https://libraryapplication-cb-gdfwakbcarfqfuda.italynorth-01.azurewebsites.net/health
```

A successful health check returns:

```text
Healthy
```

with:

```text
200 OK
```

The health check also verifies database connectivity.

---

# Swagger

Swagger is enabled in the **Development** environment.

Docker/local example:

```text
http://localhost:8080/swagger
```

Typical authentication flow:

```text
Register
   ↓
Login
   ↓
Receive Access Token
   ↓
Authorize with Bearer Token
   ↓
Use Protected Endpoints
```

Swagger is not enabled by default in the Azure production environment.

---

# Running Without Docker

From the repository root:

```bash
dotnet restore ./LibraryApi.slnx
dotnet run --project ./LibraryApi/LibraryApi.csproj
```

Local development configuration can be supplied through `.NET User Secrets`.

---

# Build

Build the complete solution:

```bash
dotnet build ./LibraryApi.slnx
```

Release build:

```bash
dotnet build ./LibraryApi.slnx --configuration Release
```

---

# Project Goals

The project was developed to demonstrate practical backend engineering concepts including:

```text
ASP.NET Core Web API
.NET 10
RESTful API Design
Entity Framework Core
SQL Server
LINQ
Authentication
Authorization
JWT
Refresh Tokens
DTOs
FluentValidation
AutoMapper
Middleware
Structured Logging
Business Rules
Pagination
Filtering
Searching
Sorting
Unit Testing
Integration Testing
Docker
Docker Compose
Health Checks
Azure App Service
Azure SQL Database
GitHub Actions
CI/CD
OpenID Connect
Cloud Deployment
```

The project goes beyond basic CRUD operations and demonstrates a realistic backend development lifecycle:

```text
Design
  ↓
Implementation
  ↓
Database
  ↓
Authentication
  ↓
Business Rules
  ↓
Automated Testing
  ↓
Containerization
  ↓
Cloud Deployment
  ↓
CI/CD
```

---

# Future Development

A frontend application is planned for the project.

The frontend will communicate with the deployed API over HTTPS and will provide separate experiences for:

```text
Members
Admins
```

The backend is designed to remain independently deployable and reusable by different clients.

---

# License

This project is intended for educational and portfolio purposes.
