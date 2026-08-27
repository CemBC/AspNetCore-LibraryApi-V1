# LibraryApi

A RESTful Library Management API built with **ASP.NET Core and .NET 10**.

The project provides authentication, role-based authorization, book and member management, loan workflows, loan requests, extension requests, pagination, validation, logging, health checks, automated tests, and Docker support.

## Technologies

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- ASP.NET Core Identity PasswordHasher
- FluentValidation
- AutoMapper
- xUnit
- EF Core InMemory
- Docker
- Docker Compose
- Swagger / OpenAPI

---

## Features

### Authentication

- User registration
- Login
- JWT access tokens
- Refresh tokens
- Refresh token rotation
- Hashed refresh tokens
- Logout
- Current authenticated user endpoint
- Role-based authorization

Roles:

```text
Admin
Member
```

---

## Book Management

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

## Member Management

Admins can:

- View members
- Update members
- Delete members
- View a member's loan history
- Search members
- Sort members
- Use pagination

Each registered member is linked to a `User` through a one-to-one relationship.

---

## Loan Management

The API supports:

- Manual loan creation by admins
- Loan creation from approved requests
- Book returns
- Active loan monitoring
- Overdue loan monitoring
- Member-specific loan history
- Search
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

## Loan Requests

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

If the admin rejects the request:

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

## Loan Extension Requests

Members can request an extension for an active loan.

If approved:

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

## Business Rules

The API enforces several library rules.

### Maximum Loans

A member can have at most:

```text
3 unreturned loans
```

The rule is checked when:

- A member creates a loan request
- An admin approves a loan request
- An admin manually creates a loan

### Overdue Loan Restriction

A member with an overdue book cannot receive or request another book.

The application checks the actual due date:

```csharp
loan.ReturnDate == null &&
loan.DueDate < DateTime.UtcNow
```

This prevents stale loan status values from bypassing the rule.

### Maximum Extensions

Each loan can have at most:

```text
2 approved extensions
```

### Overdue Extension Restriction

A loan cannot be extended after its due date has passed.

---

## Pagination, Search and Sorting

Main collection endpoints support query parameters.

Example:

```http
GET /api/Books?page=1&pageSize=10&search=Dune&status=1&sortBy=title&descending=false
```

Paged responses use the following structure:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalCount": 0,
  "totalPages": 0
}
```

Pagination is available for:

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

## Authorization

The API uses JWT Bearer authentication.

### Member Operations

Members can:

- View books
- View their own loans
- Create loan requests
- View their own pending loan requests
- Create loan extension requests
- View their own pending extension requests

### Admin Operations

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
- View admin statistics

---

## Admin Statistics

Admins can access:

```http
GET /api/Admin/statistics
```

The response includes:

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

## Global Exception Handling

The API uses custom exception middleware.

Typical mappings:

| Exception | HTTP Status |
|---|---:|
| `BadRequestException` | 400 |
| `UnauthorizedException` | 401 |
| `NotFoundException` | 404 |
| Unexpected Exception | 500 |

Unexpected errors are logged through `ILogger`.

---

## Logging

Structured logging is used for important state-changing operations.

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

Sensitive information such as passwords, password hashes, access tokens, refresh tokens, and JWT secrets is not logged.

---

# Testing

The solution contains a separate xUnit test project:

```text
LibraryApi.Tests
```

The test suite includes both unit and integration tests.

Covered areas include:

- Authentication
- JWT
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

The test suite currently contains more than 100 automated tests.

Run all tests with:

```bash
dotnet test
```

Integration tests use:

```text
WebApplicationFactory
Entity Framework Core InMemory
```

and do not modify the development SQL Server database.

---

# Docker

The API and SQL Server can run together using Docker Compose.

Architecture:

```text
Browser / Client
       │
       │ localhost:8080
       ▼
┌─────────────────────┐
│   LibraryApi        │
│   ASP.NET Core      │
│   Container         │
└─────────┬───────────┘
          │
          │ Docker network
          ▼
┌─────────────────────┐
│   SQL Server 2022   │
│   Container         │
└─────────┬───────────┘
          │
          ▼
    Docker Volume
```

---

## Docker Requirements

Install:

- Docker Desktop
- WSL 2 on Windows

Verify Docker:

```bash
docker --version
```

You can also test the Docker engine with:

```bash
docker run hello-world
```

---

## Environment Variables

Copy:

```text
.env.example
```

and create:

```text
.env
```

Example:

```env
MSSQL_SA_PASSWORD=YourStrongPasswordHere123!

JWT_KEY=YourJwtSecretKeyHere
JWT_ISSUER=LibraryApi
JWT_AUDIENCE=LibraryApiClient
```

The `.env` file is excluded from Git.

Never commit real passwords or JWT secrets.

---

## Start with Docker Compose

Build and start the API and SQL Server:

```bash
docker compose up -d --build
```

If the application code has not changed and the image already exists:

```bash
docker compose up -d
```

Check running services:

```bash
docker compose ps
```

---

## Stop Docker Services

When development is finished:

```bash
docker compose down
```

This removes the API and SQL Server containers but keeps the database volume.

Database data remains available the next time the application starts.

Do **not** normally use:

```bash
docker compose down -v
```

because `-v` also deletes the database volume.

---

## Docker Logs

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

## Docker Resource Usage

View running container resource usage:

```bash
docker stats
```

View Docker disk usage:

```bash
docker system df
```

View Docker volumes:

```bash
docker volume ls
```

When Docker is no longer needed, Docker Desktop can be closed.

On Windows, WSL can also be completely stopped with:

```powershell
wsl --shutdown
```

Docker Desktop will start its WSL backend again the next time Docker is launched.

---

## SQL Server

Docker exposes SQL Server on:

```text
localhost,1433
```

It can be accessed from SQL Server Management Studio using:

```text
Server: localhost,1433
Authentication: SQL Server Authentication
Login: sa
Password: value from MSSQL_SA_PASSWORD
```

If necessary, enable:

```text
Trust Server Certificate
```

Inside the Docker network, the API does not use `localhost`.

It connects to SQL Server through the Compose service name:

```text
Server=sqlserver,1433
```

---

## Database Persistence

SQL Server data is stored in a Docker volume.

This means:

```bash
docker compose down
```

does not delete:

```text
Users
Members
Books
Loans
Loan Requests
Extension Requests
```

The volume consumes disk space only while containers are stopped; it does not consume CPU or RAM by itself.

---

# Entity Framework Core Migrations

Database schema changes are managed with EF Core migrations.

Create a migration:

```bash
dotnet ef migrations add MigrationName
```

Apply migrations:

```bash
dotnet ef database update
```

For a Docker SQL Server running on port `1433`, the connection string can temporarily be overridden from PowerShell:

```powershell
$env:ConnectionStrings__LibraryDb="Server=localhost,1433;Database=LibraryDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=True"
```

Then run:

```powershell
dotnet ef database update
```

Afterwards, remove the temporary environment variable:

```powershell
Remove-Item Env:ConnectionStrings__LibraryDb
```

---

# Health Checks

The API exposes:

```http
GET /health
```

Example:

```text
http://localhost:8080/health
```

A healthy application returns:

```text
Healthy
```

with:

```text
200 OK
```

The health check also verifies database connectivity.

If SQL Server becomes unavailable, the health endpoint reports the application as unhealthy.

---

# Swagger

When running in the Development environment:

```text
http://localhost:8080/swagger
```

Swagger can be used to test authentication and API endpoints.

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

---

# Useful Docker Commands

```bash
# Start
docker compose up -d

# Start and rebuild after code changes
docker compose up -d --build

# Stop everything while keeping database data
docker compose down

# Check Compose services
docker compose ps

# Check all running containers
docker ps

# Check all containers
docker ps -a

# View API logs
docker compose logs api

# Follow logs
docker compose logs -f

# Check RAM and CPU usage
docker stats

# List images
docker images

# List volumes
docker volume ls

# Check Docker disk usage
docker system df
```

---

# Running Without Docker

The project can also be run directly with .NET:

```bash
dotnet restore
dotnet run
```

In this mode, the connection string from the local application configuration is used.

---

# Project Goals

LibraryApi was built as a practical backend project demonstrating:

```text
ASP.NET Core Web API
RESTful API Design
Entity Framework Core
SQL Server
Authentication
Authorization
JWT
Refresh Tokens
DTOs
Validation
AutoMapper
Middleware
Logging
Business Rules
Pagination
Filtering
Sorting
Automated Testing
Docker
Docker Compose
Health Checks
```

The project is designed to demonstrate a realistic backend architecture rather than only basic CRUD operations.