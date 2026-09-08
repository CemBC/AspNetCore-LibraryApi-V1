# Library Management System API

A full-featured Library Management REST API built with **ASP.NET Core and .NET 10**.

The project demonstrates a complete backend development workflow including authentication, authorization, business rules, automated testing, image storage, containerization, cloud deployment, database hosting, health checks, API documentation, and CI/CD.

The API is deployed to **Microsoft Azure App Service**, uses **Azure SQL Database** for production data, and **Azure Blob Storage** for book cover images.

A separate **React + TypeScript frontend** consumes the production API.

---

# Live Deployment

## API

The production API is hosted on Microsoft Azure App Service:

https://libraryapplication-cb-gdfwakbcarfqfuda.italynorth-01.azurewebsites.net

## Swagger UI

https://libraryapplication-cb-gdfwakbcarfqfuda.italynorth-01.azurewebsites.net/swagger

Swagger provides an interactive interface for exploring and testing the API endpoints.

## Scalar API Reference

https://libraryapplication-cb-gdfwakbcarfqfuda.italynorth-01.azurewebsites.net/scalar/v1

Scalar provides a modern API reference interface for browsing the available endpoints and schemas.

## Health Check

https://libraryapplication-cb-gdfwakbcarfqfuda.italynorth-01.azurewebsites.net/health

A healthy deployment returns:

```text
Healthy
```

The health check also verifies connectivity to the production database.

---

# Frontend

A complete frontend application is available in a separate repository.

Repository:

https://github.com/CemBC/LibraryManagement-website

The frontend is built with:

- React
- TypeScript
- Vite
- Axios
- React Router

It provides separate interfaces for **Members** and **Admins** and communicates with this API over HTTPS.

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
- Dependency Injection
- Service Abstractions

## Cloud Storage

- Azure Blob Storage
- Book Cover Image Uploads

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
- Azure Blob Storage

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
│   │   └── Interfaces/
│   ├── Settings/
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

# Architecture

The application follows a layered architecture.

```text
Client
   │
   ▼
Controllers
   │
   ▼
Service Interfaces
   │
   ▼
Services
   │
   ▼
Entity Framework Core
   │
   ▼
Database
```

Controllers are responsible for HTTP communication while business rules are implemented in the service layer.

Services are consumed through interfaces such as:

```text
IBookService
IMemberService
ILoanService
ILoanRequestService
ILoanExtensionService
IAuthService
IAdminService
IBlobStorageService
IEmailService
```

ASP.NET Core Dependency Injection resolves their concrete implementations.

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
- Upload book cover images
- View books

Members can:

- View books
- Search books
- Filter books by status
- View book details
- Use pagination

Book statuses:

```text
Available
Requested
Loaned
```

Book cover images can be stored in Azure Blob Storage and served through their public image URLs.

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
   │
   ▼
Creates Loan Request
   │
   ▼
Book → Requested
   │
   ▼
Admin Approves
   │
   ▼
Loan → Active
   │
   ▼
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

## Overdue Loan Restriction

Members with overdue loans cannot receive or request another book.

Overdue detection is based on the actual loan state:

```csharp
loan.ReturnDate == null &&
loan.DueDate < DateTime.UtcNow
```

This prevents stale status values from bypassing the business rule.

## Maximum Extensions

Each loan can have at most:

```text
2 approved extensions
```

## Overdue Extension Restriction

A loan cannot be extended after its due date has passed.

## Relationship Protection

Books and members that are referenced by relevant loan records cannot be deleted in ways that would violate application consistency.

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

Pagination is implemented for areas including:

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
- Manage their authenticated session

## Admin Operations

Admins can:

- Manage books
- Upload book covers
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

These statistics are consumed by the frontend admin dashboard.

---

# Book Cover Storage

Book cover images can be uploaded through the API.

Example endpoint:

```http
POST /api/Books/{id}/image
```

The frontend sends the image using `multipart/form-data`.

In production, images are stored using:

```text
Azure Blob Storage
```

The resulting image URL is associated with the book and can be displayed directly by frontend clients.

This keeps binary image data outside the relational database.

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

Invalid requests return appropriate:

```text
400 Bad Request
```

responses.

---

# DTO Mapping

The application uses **AutoMapper** to map between application entities and DTOs.

```text
Database Entities
       │
       ▼
    AutoMapper
       │
       ▼
      DTOs
       │
       ▼
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

This keeps controller code focused on request handling rather than repeated exception handling logic.

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
Database credentials
```

---

# Automated Testing

The repository contains a separate test project:

```text
LibraryApi.Tests
```

The automated test suite contains both:

```text
Unit Tests
Integration Tests
```

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

# Integration Testing

Integration tests use:

```text
WebApplicationFactory
Entity Framework Core InMemory
```

The integration environment uses an isolated in-memory database.

Therefore automated tests do not modify the development or production SQL Server database.

---

# Run Tests

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
      │
      ▼
Checkout Repository
      │
      ▼
Restore Dependencies
      │
      ▼
Build Solution
      │
      ▼
Run Automated Tests
      │
      ▼
Publish API
      │
      ▼
Create Deployment Artifact
      │
      ▼
Authenticate to Azure using OIDC
      │
      ▼
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

The deployment identity only has access to the required Azure resources.

---

# Production Architecture

The complete deployed application consists of a separate frontend, API, relational database, and blob storage.

```text
┌──────────────────────────────┐
│       GitHub Pages           │
│                              │
│ React + TypeScript Frontend  │
└──────────────┬───────────────┘
               │
               │ HTTPS / REST
               ▼
┌──────────────────────────────┐
│     Azure App Service        │
│                              │
│ ASP.NET Core / .NET 10 API   │
└──────────────┬───────────────┘
               │
               ├──────────────────────┐
               │                      │
               ▼                      ▼
┌──────────────────────┐   ┌──────────────────────┐
│ Azure SQL Database   │   │ Azure Blob Storage   │
│                      │   │                      │
│ Application Data     │   │ Book Cover Images    │
└──────────────────────┘   └──────────────────────┘
```

The frontend and backend are independently deployable.

---

# CORS

The API supports a separately hosted frontend using ASP.NET Core CORS configuration.

The allowed frontend origin is supplied through configuration:

```text
Frontend:Url
```

For local development, the default frontend origin is:

```text
http://localhost:5173
```

Production uses the deployed frontend origin.

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
Storage credentials
Email credentials
```

must never be committed to Git.

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

Production:

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

The frontend uses this endpoint to display the current API availability.

---

# Docker

The API and SQL Server can run together using Docker Compose.

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

# Docker Environment Variables

Docker configuration uses:

```text
.env
```

A safe example configuration can be supplied through:

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

---


Typical authentication flow:

```text
Register
   │
   ▼
Login
   │
   ▼
Receive Access Token
   │
   ▼
Authorize with Bearer Token
   │
   ▼
Use Protected Endpoints
```

JWT Bearer authentication is configured directly in the Swagger interface.

---

# Scalar

The API also provides a Scalar API reference.

```text
/scalar/v1
```

Scalar provides another interface for exploring the generated OpenAPI specification.

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

# Full Stack Integration

This API is consumed by the Library Management frontend:

```text
React Frontend
      │
      │ Axios
      │ HTTPS
      ▼
ASP.NET Core REST API
      │
      ▼
Application Services
      │
      ▼
Entity Framework Core
      │
      ▼
Azure SQL Database
```

Authentication is shared through JWT access tokens issued by this API.

Frontend authorization adapts the interface according to the authenticated user's role.

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
Dependency Injection
Service Abstractions
Middleware
Structured Logging
Business Rules
Pagination
Filtering
Searching
Sorting
File Uploads
Azure Blob Storage
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
Frontend Integration
```

The project goes beyond basic CRUD operations and demonstrates a realistic backend development lifecycle:

```text
Design
  │
  ▼
Implementation
  │
  ▼
Database
  │
  ▼
Authentication
  │
  ▼
Business Rules
  │
  ▼
Automated Testing
  │
  ▼
Containerization
  │
  ▼
Cloud Deployment
  │
  ▼
CI/CD
  │
  ▼
Frontend Integration
```

---

# Related Repository

Frontend application:

https://github.com/CemBC/LibraryManagement-website

---

# License

This project is intended for educational and portfolio purposes.
