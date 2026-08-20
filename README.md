# Library API

Library API is a simple library management REST API developed with ASP.NET Core Web API, Entity Framework Core, and SQL Server.

The project was created to practice controller-based API development, dependency injection, service-layer separation, CRUD operations, HTTP status codes, Entity Framework Core, LINQ, database relationships, migrations, asynchronous database operations, DTOs, and basic business rules.

## Features

### Book Management

- List all books
- Get a book by ID
- Add a new book
- Update an existing book
- Delete a book
- Track whether a book is currently available

### Member Management

- List all members
- Get a member by ID
- Add a new member
- Update an existing member
- Delete a member

### Loan Management

- List all loan records
- Get a loan record by ID
- Loan a book to a member
- Return a borrowed book
- Prevent unavailable books from being borrowed again
- Prevent the same loan record from being returned more than once
- Preserve returned loan records as borrowing history
- Automatically update book availability when a book is borrowed or returned

## Technologies

- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- SQL Server LocalDB
- EF Core SQL Server Provider
- LINQ
- Code First
- EF Core Migrations
- Fluent API
- Dependency Injection
- Async/Await
- DTOs
- Controller-based API
- Visual Studio
- HTTP request files

## Database

The application uses SQL Server with Entity Framework Core for persistent data storage.

The database contains three main tables:

- `Books`
- `Members`
- `Loans`

The `Loans` table is connected to the other tables using foreign key relationships:

- `Loans.BookId` → `Books.Id`
- `Loans.MemberId` → `Members.Id`

The relationships are:

```text
Book 1 ─── N Loan
Member 1 ─ N Loan
```

This means that a book can have multiple loan records over time, and a member can also have multiple loan records.

Primary keys are generated automatically by SQL Server.

The default LocalDB database name is:

```text
LibraryEfDb
```

## Entity Framework Core

The project uses Entity Framework Core instead of direct SQL commands.

The main EF Core concepts used in the project include:

- `DbContext`
- `DbSet<T>`
- Code First
- Migrations
- LINQ queries
- Change Tracking
- `AsNoTracking()`
- Fluent API configuration
- Relationships and foreign keys
- Seed data
- Asynchronous database operations
- `SaveChangesAsync()`

The application's database model is managed through:

```text
Models
    ↓
Fluent API Configurations
    ↓
DbContext
    ↓
Migrations
    ↓
SQL Server
```

## Migrations

Database creation and schema changes are managed using Entity Framework Core migrations.

The repository includes the migrations required to create the database schema.

To apply the migrations:

```bash
dotnet ef database update
```

If the database does not exist, EF Core creates it automatically and applies the migrations.

The initial migration also inserts the configured seed data.

## Seed Data

The application includes initial sample data configured with EF Core.

Example books:

- Suç ve Ceza — Fyodor Dostoyevski
- 1984 — George Orwell

Example members:

- John Doe
- Jane Smith

The seed data is applied through EF Core migrations.

## Data Access

Database operations are handled by the service layer using Entity Framework Core.

For read-only queries, the project uses operations such as:

```csharp
AsNoTracking()
ToListAsync()
FirstOrDefaultAsync()
AnyAsync()
```

For create, update, and delete operations, the project uses EF Core change tracking together with:

```csharp
AddAsync()
Remove()
SaveChangesAsync()
```

For example, when a book is borrowed:

1. The book is retrieved from the database.
2. The application checks whether the book is available.
3. A new `Loan` entity is created.
4. The book's `IsAvailable` property is changed to `false`.
5. Both changes are persisted using a single `SaveChangesAsync()` call.

Similarly, returning a book updates both the loan record and the book availability.

## DTOs

DTOs are used to separate the API contract from Entity Framework Core entities.

For example, creating a loan only requires:

```json
{
  "bookId": 1,
  "memberId": 1
}
```

The API does not require clients to provide Entity Framework navigation properties such as `Book` or `Member`.

Loan responses are also mapped to a dedicated response DTO.

This prevents the persistence model from being exposed directly through the API and avoids circular JSON serialization caused by bidirectional navigation properties.

## Project Structure

```text
LibraryApi
├── Controllers
│   ├── BooksController.cs
│   ├── MembersController.cs
│   └── LoansController.cs
│
├── Data
│   ├── LibraryDbContext.cs
│   │
│   └── Configurations
│       ├── BookConfiguration.cs
│       ├── MemberConfiguration.cs
│       └── LoanConfiguration.cs
│
├── DTOs
│   └── Loans
│       ├── CreateLoanRequest.cs
│       └── LoanResponse.cs
│
├── Models
│   ├── Book.cs
│   ├── Member.cs
│   ├── Loan.cs
│   └── LoanOperationStatus.cs
│
├── Services
│   ├── BookService.cs
│   ├── MemberService.cs
│   └── LoanService.cs
│
├── Migrations
│
├── Properties
│   └── launchSettings.json
│
├── LibraryApi.http
├── Program.cs
├── appsettings.json
└── LibraryApi.csproj
```

## Architecture

The application follows a simple layered structure:

```text
HTTP Request
     ↓
Controller
     ↓
DTO / Entity Mapping
     ↓
Service
     ↓
Entity Framework Core
     ↓
SQL Server
```

Controllers handle HTTP requests and responses.

Services contain application logic and interact with the Entity Framework Core `DbContext`.

Entity Framework Core handles SQL generation, change tracking, relationships, and persistence.

DTOs are used where the API contract should be separated from database entities.

## API Endpoints

### Books

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/books` | Get all books |
| GET | `/api/books/{id}` | Get a book by ID |
| POST | `/api/books` | Create a new book |
| PUT | `/api/books/{id}` | Update a book |
| DELETE | `/api/books/{id}` | Delete a book |

### Members

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/members` | Get all members |
| GET | `/api/members/{id}` | Get a member by ID |
| POST | `/api/members` | Create a new member |
| PUT | `/api/members/{id}` | Update a member |
| DELETE | `/api/members/{id}` | Delete a member |

### Loans

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/loans` | Get all loan records |
| GET | `/api/loans/{id}` | Get a loan record by ID |
| POST | `/api/loans` | Loan a book to a member |
| PUT | `/api/loans/{id}/return` | Return a borrowed book |

## Requirements

Make sure the following are installed:

- .NET 10 SDK
- SQL Server LocalDB
- Entity Framework Core CLI tools
- SQL Server Management Studio or another SQL Server management tool (optional)

The project uses the default SQL Server LocalDB instance:

```text
(localdb)\MSSQLLocalDB
```

## Setup

Clone the repository:

```bash
git clone https://github.com/CemBC/AspNetCore-LibraryApi-V1.git
```

Navigate to the project directory:

```bash
cd AspNetCore-LibraryApi-V1
```

Restore NuGet packages:

```bash
dotnet restore
```

If the EF Core CLI tool is not installed:

```bash
dotnet tool install --global dotnet-ef
```

Apply the database migrations:

```bash
dotnet ef database update
```

Run the API:

```bash
dotnet run
```

The database will be created automatically from the EF Core migrations.

## Connection String

The SQL Server connection string is configured in `appsettings.json`.

Example:

```json
{
  "ConnectionStrings": {
    "LibraryDb": "Server=(localdb)\\MSSQLLocalDB;Database=LibraryEfDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

The connection string is registered with `LibraryDbContext` through dependency injection in `Program.cs`.

## Dependency Injection

`LibraryDbContext` is registered using:

```csharp
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("LibraryDb")));
```

The application services are registered with a scoped lifetime:

```csharp
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<LoanService>();
```

This allows each HTTP request to use its own scoped `DbContext` instance.

## Business Rules

The API currently enforces several basic library rules:

- A loan cannot be created for a book that does not exist.
- A loan cannot be created for a member that does not exist.
- An unavailable book cannot be borrowed again.
- Borrowing a book sets `IsAvailable` to `false`.
- Returning a book sets `IsAvailable` back to `true`.
- A loan cannot be returned more than once.
- Loan records are preserved after a book is returned.
- Existing loan relationships prevent related historical data from being silently cascade-deleted.
