# Library API

Library API is a simple library management REST API developed with ASP.NET Core Web API and SQL Server.

The project was created to practice controller-based API development, dependency injection, service-layer separation, CRUD operations, HTTP status codes, SQL database integration, ADO.NET, transactions, and basic business rules.

## Features

### Book Management

* List all books
* Get a book by ID
* Add a new book
* Update an existing book
* Delete a book
* Track whether a book is currently available

### Member Management

* List all members
* Get a member by ID
* Add a new member
* Update an existing member
* Delete a member

### Loan Management

* List all loan records
* Get a loan record by ID
* Loan a book to a member
* Return a borrowed book
* Prevent unavailable books from being borrowed again
* Prevent the same loan record from being returned more than once
* Preserve returned loan records as borrowing history
* Keep loan and book availability updates consistent using SQL transactions

## Technologies

* C#
* .NET 10
* ASP.NET Core Web API
* SQL Server
* SQL Server LocalDB
* ADO.NET
* Microsoft.Data.SqlClient
* Controller-based API
* Dependency Injection
* Parameterized SQL queries
* SQL transactions
* Visual Studio
* HTTP request files

## Database

The application uses SQL Server for persistent data storage.

The database contains three main tables:

* `Books`
* `Members`
* `Loans`

The `Loans` table is connected to the other tables using foreign keys:

* `Loans.BookId` → `Books.Id`
* `Loans.MemberId` → `Members.Id`

Book IDs, member IDs, and loan IDs are generated automatically by SQL Server using `IDENTITY`.

Loan creation and return operations use SQL transactions to ensure that loan records and book availability remain consistent.

## Database Setup

The repository includes a SQL setup script:

```text
Database/setup.sql
```

The script creates:

* `LibraryDb` database
* `Books` table
* `Members` table
* `Loans` table
* Primary keys
* Foreign key relationships
* Initial sample books
* Initial sample members

### Requirements

Make sure the following are installed:

* .NET 10 SDK
* SQL Server LocalDB
* SQL Server Management Studio (recommended)

The default database connection uses:

```text
(localdb)\MSSQLLocalDB
```

### Setup Steps

1. Clone the repository.

```bash
git clone https://github.com/CemBC/AspNetCore-LibraryApi-V1.git
```

2. Navigate to the project directory.

```bash
cd AspNetCore-LibraryApi-V1
```

3. Restore NuGet packages.

```bash
dotnet restore
```

4. Open `Database/setup.sql` in SQL Server Management Studio.

5. Connect to:

```text
(localdb)\MSSQLLocalDB
```

6. Execute the setup script.

7. Run the API.

```bash
dotnet run
```

## Data Access

The service layer communicates directly with SQL Server using ADO.NET and `Microsoft.Data.SqlClient`.

The project uses SQL operations such as:

* `SELECT`
* `INSERT`
* `UPDATE`
* `DELETE`
* `OUTPUT INSERTED.Id`
* Parameterized queries
* Primary keys
* Foreign keys
* Identity-generated IDs
* SQL transactions

Different ADO.NET execution methods are used depending on the operation:

* `ExecuteReader()` for reading multiple rows
* `ExecuteScalar()` for retrieving a single value
* `ExecuteNonQuery()` for `INSERT`, `UPDATE`, and `DELETE` operations

Entity Framework Core is not used in this version of the project.

## Project Structure

```text
LibraryApi
├── Controllers
│   ├── BooksController.cs
│   ├── MembersController.cs
│   └── LoansController.cs
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
├── Database
│   └── setup.sql
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
Service
     ↓
ADO.NET
     ↓
SQL Server
```

Controllers handle HTTP requests and responses, while services contain the application logic and database operations.

This separation allows the data storage implementation to change without requiring major changes to the controller layer.
