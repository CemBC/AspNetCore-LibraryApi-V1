IF DB_ID(N'LibraryDb') IS NULL
BEGIN
    CREATE DATABASE LibraryDb;
END
GO

USE LibraryDb;
GO

IF OBJECT_ID(N'dbo.Books', N'U') IS NULL
BEGIN
    CREATE TABLE Books
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Author NVARCHAR(150) NOT NULL,
        IsAvailable BIT NOT NULL DEFAULT 1
    );
END
GO

IF OBJECT_ID(N'dbo.Members', N'U') IS NULL
BEGIN
    CREATE TABLE Members
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Email NVARCHAR(200) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Loans', N'U') IS NULL
BEGIN
    CREATE TABLE Loans
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        BookId INT NOT NULL,
        MemberId INT NOT NULL,
        LoanDate DATETIME2 NOT NULL,
        ReturnDate DATETIME2 NULL,

        FOREIGN KEY (BookId) REFERENCES Books(Id),
        FOREIGN KEY (MemberId) REFERENCES Members(Id)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM Books)
BEGIN
    INSERT INTO Books (Title, Author)
    VALUES
        (N'Suç ve Ceza', N'Fyodor Dostoyevski'),
        (N'1984', N'George Orwell');
END
GO

IF NOT EXISTS (SELECT 1 FROM Members)
BEGIN
    INSERT INTO Members (FullName, Email)
    VALUES
        (N'John Doe', N'john.doe@example.com'),
        (N'Jane Smith', N'jane.smith@example.com');
END
GO