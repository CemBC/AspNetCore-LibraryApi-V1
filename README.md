# Library API

Library API is a simple library management REST API developed with ASP.NET Core Web API.

The project was created to practice controller-based API development, dependency injection, service-layer separation, CRUD operations, HTTP status codes, and basic business rules.

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

## Technologies

- C#
- .NET 10
- ASP.NET Core Web API
- Controller-based API
- Dependency Injection
- In-memory `List<T>` collections
- Visual Studio
- HTTP request files

## Project Structure

```text
LibraryApi
├── Controllers
│   ├── BooksController.cs
│   ├── MembersController.cs
│   └── LoansController.cs
├── Models
│   ├── Book.cs
│   ├── Member.cs
│   ├── Loan.cs
│   └── LoanOperationStatus.cs
├── Services
│   ├── BookService.cs
│   ├── MemberService.cs
│   └── LoanService.cs
├── Properties
│   └── launchSettings.json
├── LibraryApi.http
├── Program.cs
├── appsettings.json
└── LibraryApi.csproj
