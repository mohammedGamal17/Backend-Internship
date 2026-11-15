/*
    Library Management System - SQL Server seed database
    Target: Microsoft SQL Server 2019+
    Usage:
      1. Open this file in SQL Server Management Studio (SSMS).
      2. Execute the script.
      3. Connect your C# application to: LibraryManagementDB
         Example: Server=.;Database=LibraryManagementDB;Trusted_Connection=True;TrustServerCertificate=True;
    Note: This script creates the database if it does not exist. It does not drop or overwrite it.
*/

IF DB_ID(N'LibraryManagementDB') IS NULL
BEGIN
    CREATE DATABASE LibraryManagementDB;
END;
GO

USE LibraryManagementDB;
GO

SET NOCOUNT ON;
GO

/* Tables */
IF OBJECT_ID(N'dbo.LoanItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LoanItems
    (
        LoanItemId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoanItems PRIMARY KEY,
        LoanId INT NOT NULL,
        BookId INT NOT NULL,
        DueDate DATE NOT NULL,
        ReturnedDate DATE NULL,
        CONSTRAINT UQ_LoanItems_Loan_Book UNIQUE (LoanId, BookId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Loans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Loans
    (
        LoanId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Loans PRIMARY KEY,
        MemberId INT NOT NULL,
        LoanDate DATE NOT NULL,
        Status VARCHAR(20) NOT NULL CONSTRAINT DF_Loans_Status DEFAULT ('Active'),
        CONSTRAINT CK_Loans_Status CHECK (Status IN ('Active', 'Returned', 'Overdue'))
    );
END;
GO

IF OBJECT_ID(N'dbo.Books', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Books
    (
        BookId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Books PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Author NVARCHAR(150) NOT NULL,
        CategoryId INT NOT NULL,
        PublishedYear SMALLINT NULL,
        IsAvailable BIT NOT NULL CONSTRAINT DF_Books_IsAvailable DEFAULT (1),
        TotalCopies INT NOT NULL CONSTRAINT DF_Books_TotalCopies DEFAULT (1),
        CONSTRAINT CK_Books_TotalCopies CHECK (TotalCopies >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.Members', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Members
    (
        MemberId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Members PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Email NVARCHAR(254) NOT NULL,
        JoinDate DATE NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Members_IsActive DEFAULT (1),
        CONSTRAINT UQ_Members_Email UNIQUE (Email)
    );
END;
GO

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        CategoryId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL,
        CONSTRAINT UQ_Categories_CategoryName UNIQUE (CategoryName)
    );
END;
GO

/* Foreign keys are added after all tables exist. */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Books_Categories')
    ALTER TABLE dbo.Books ADD CONSTRAINT FK_Books_Categories
        FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(CategoryId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Loans_Members')
    ALTER TABLE dbo.Loans ADD CONSTRAINT FK_Loans_Members
        FOREIGN KEY (MemberId) REFERENCES dbo.Members(MemberId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_LoanItems_Loans')
    ALTER TABLE dbo.LoanItems ADD CONSTRAINT FK_LoanItems_Loans
        FOREIGN KEY (LoanId) REFERENCES dbo.Loans(LoanId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_LoanItems_Books')
    ALTER TABLE dbo.LoanItems ADD CONSTRAINT FK_LoanItems_Books
        FOREIGN KEY (BookId) REFERENCES dbo.Books(BookId);
GO

/* Seed data: insert only when the tables are empty, so re-running is safe. */
IF NOT EXISTS (SELECT 1 FROM dbo.Categories)
BEGIN
    INSERT INTO dbo.Categories (CategoryName)
    VALUES
        (N'Programming'), (N'Fiction'), (N'Science'), (N'History'),
        (N'Business'), (N'Children'), (N'Biography'), (N'Philosophy');
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Books)
BEGIN
    INSERT INTO dbo.Books (Title, Author, CategoryId, PublishedYear, IsAvailable, TotalCopies)
    SELECT v.Title, v.Author, c.CategoryId, v.PublishedYear, v.IsAvailable, v.TotalCopies
    FROM (VALUES
        (N'Clean Code', N'Robert C. Martin', N'Programming', 2008, 1, 4),
        (N'The Pragmatic Programmer', N'Andrew Hunt and David Thomas', N'Programming', 1999, 1, 3),
        (N'C# in Depth', N'Jon Skeet', N'Programming', 2019, 1, 2),
        (N'Design Patterns', N'Erich Gamma and others', N'Programming', 1994, 0, 2),
        (N'1984', N'George Orwell', N'Fiction', 1949, 1, 5),
        (N'To Kill a Mockingbird', N'Harper Lee', N'Fiction', 1960, 1, 3),
        (N'The Hobbit', N'J.R.R. Tolkien', N'Fiction', 1937, 1, 4),
        (N'Dune', N'Frank Herbert', N'Fiction', 1965, 0, 2),
        (N'A Brief History of Time', N'Stephen Hawking', N'Science', 1988, 1, 2),
        (N'Cosmos', N'Carl Sagan', N'Science', 1980, 1, 3),
        (N'Sapiens', N'Yuval Noah Harari', N'History', 2011, 1, 4),
        (N'The Silk Roads', N'Peter Frankopan', N'History', 2015, 1, 2),
        (N'The Lean Startup', N'Eric Ries', N'Business', 2011, 1, 2),
        (N'Atomic Habits', N'James Clear', N'Business', 2018, 1, 5),
        (N'Charlotte''s Web', N'E.B. White', N'Children', 1952, 1, 3),
        (N'The Little Prince', N'Antoine de Saint-Exupéry', N'Children', 1943, 1, 4),
        (N'Steve Jobs', N'Walter Isaacson', N'Biography', 2011, 1, 2),
        (N'Meditations', N'Marcus Aurelius', N'Philosophy', 2002, 1, 3),
        (N'Empty Shelf Example', N'Unknown Author', N'Fiction', 2020, 1, 0),
        (N'Rare Book Example', N'Unknown Author', N'History', 2022, 0, 1)
    ) AS v(Title, Author, CategoryName, PublishedYear, IsAvailable, TotalCopies)
    INNER JOIN dbo.Categories c ON c.CategoryName = v.CategoryName;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Members)
BEGIN
    INSERT INTO dbo.Members (FullName, Email, JoinDate, IsActive)
    VALUES
        (N'Ahmed Hassan', N'ahmed.hassan@example.com', '2024-01-15', 1),
        (N'Mohammed Gamal', N'mohammed.gamal@example.com', '2024-04-01', 1),
        (N'Mariam Ali', N'mariam.ali@example.com', '2024-02-20', 1),
        (N'Omar Khaled', N'omar.khaled@example.com', '2024-03-05', 1),
        (N'Sara Mohamed', N'sara.mohamed@example.com', '2024-04-12', 1),
        (N'Youssef Adel', N'youssef.adel@example.com', '2024-05-25', 0),
        (N'Nour Ibrahim', N'nour.ibrahim@example.com', '2024-06-10', 1),
        (N'Karim Mostafa', N'karim.mostafa@example.com', '2024-07-01', 1),
        (N'Farah Samir', N'farah.samir@example.com', '2024-08-19', 1),
        (N'Ziad Ashraf', N'ziad.ashraf@example.com', '2024-09-03', 1),
        (N'Layla Tarek', N'layla.tarek@example.com', '2024-10-11', 1),
        (N'Inactive Member Example', N'inactive.member@example.com', '2023-12-01', 0),
        (N'No Loans Member', N'no.loans@example.com', '2025-01-05', 1)
END;
GO

/* Loans and loan items use explicit IDs for predictable sample relationships.
   Insert them only when no loans exist. */
IF NOT EXISTS (SELECT 1 FROM dbo.Loans)
BEGIN
    SET IDENTITY_INSERT dbo.Loans ON;
    INSERT INTO dbo.Loans (LoanId, MemberId, LoanDate, Status)
    VALUES
        (1, DATEADD(DAY, -3, CAST(GETDATE() AS DATE)), 'Active'),
        (2, DATEADD(DAY, -20, CAST(GETDATE() AS DATE)), 'Overdue'),
        (3, DATEADD(DAY, -10, CAST(GETDATE() AS DATE)), 'Returned'),
        (4, DATEADD(DAY, -1, CAST(GETDATE() AS DATE)), 'Active'),
        (5, DATEADD(DAY, -35, CAST(GETDATE() AS DATE)), 'Overdue'),
        (6, DATEADD(DAY, -8, CAST(GETDATE() AS DATE)), 'Returned'),
        (7, DATEADD(DAY, -5, CAST(GETDATE() AS DATE)), 'Active'),
        (8, DATEADD(DAY, -15, CAST(GETDATE() AS DATE)), 'Overdue');
    SET IDENTITY_INSERT dbo.Loans OFF;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.LoanItems)
BEGIN
    INSERT INTO dbo.LoanItems (LoanId, BookId, DueDate, ReturnedDate)
    VALUES
        (1, 1, DATEADD(DAY, 11, CAST(GETDATE() AS DATE)), NULL),
        (1, 3, DATEADD(DAY, 11, CAST(GETDATE() AS DATE)), NULL),
        (2, 5, DATEADD(DAY, -6, CAST(GETDATE() AS DATE)), NULL),
        (2, 7, DATEADD(DAY, -6, CAST(GETDATE() AS DATE)), NULL),
        (3, 2, DATEADD(DAY, -3, CAST(GETDATE() AS DATE)), DATEADD(DAY, -4, CAST(GETDATE() AS DATE))),
        (4, 9, DATEADD(DAY, 13, CAST(GETDATE() AS DATE)), NULL),
        (5, 11, DATEADD(DAY, -25, CAST(GETDATE() AS DATE)), NULL),
        (6, 13, DATEADD(DAY, -1, CAST(GETDATE() AS DATE)), DATEADD(DAY, -2, CAST(GETDATE() AS DATE))),
        (7, 14, DATEADD(DAY, 9, CAST(GETDATE() AS DATE)), NULL),
        (8, 15, DATEADD(DAY, -1, CAST(GETDATE() AS DATE)), NULL);
END;
GO

/* Useful indexes for common lookups */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Books_Title' AND object_id = OBJECT_ID(N'dbo.Books'))
    CREATE INDEX IX_Books_Title ON dbo.Books(Title);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Books_CategoryId' AND object_id = OBJECT_ID(N'dbo.Books'))
    CREATE INDEX IX_Books_CategoryId ON dbo.Books(CategoryId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Loans_MemberId_Status' AND object_id = OBJECT_ID(N'dbo.Loans'))
    CREATE INDEX IX_Loans_MemberId_Status ON dbo.Loans(MemberId, Status);
GO

/* Quick verification queries */
SELECT N'Categories' AS TableName, COUNT(*) AS RowCount FROM dbo.Categories
UNION ALL SELECT N'Books', COUNT(*) FROM dbo.Books
UNION ALL SELECT N'Members', COUNT(*) FROM dbo.Members
UNION ALL SELECT N'Loans', COUNT(*) FROM dbo.Loans
UNION ALL SELECT N'LoanItems', COUNT(*) FROM dbo.LoanItems;
GO

SELECT TOP (10)
    b.BookId, b.Title, b.Author, c.CategoryName, b.PublishedYear,
    b.IsAvailable, b.TotalCopies
FROM dbo.Books b
INNER JOIN dbo.Categories c ON c.CategoryId = b.CategoryId
ORDER BY b.Title;
GO
