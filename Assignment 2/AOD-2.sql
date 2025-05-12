-- Create the database
CREATE DATABASE LibraryDB;

-- Switch to the created database
USE LibraryDB;

-- ===========================
-- Create Tables
-- ===========================

-- Authors Table
CREATE TABLE Authors (
    AuthorID INT IDENTITY PRIMARY KEY,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    DateOfBirth DATE
);

-- Books Table
CREATE TABLE Books (
    BookID INT IDENTITY PRIMARY KEY,
    Title NVARCHAR(255),
    AuthorID INT,
    Genre NVARCHAR(50),
    PublicationYear INT,
    FOREIGN KEY (AuthorID) REFERENCES Authors(AuthorID)
);

-- Borrowers Table
CREATE TABLE Borrowers (
    BorrowerID INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100),
    Email NVARCHAR(100),
    ContactNumber NVARCHAR(15)
);

-- Borrowing Records Table
CREATE TABLE BorrowingRecords (
    BorrowingRecordID INT IDENTITY PRIMARY KEY,
    BookID INT,
    BorrowerID INT,
    BorrowDate DATE,
    ReturnDate DATE,
    FOREIGN KEY (BookID) REFERENCES Books(BookID),
    FOREIGN KEY (BorrowerID) REFERENCES Borrowers(BorrowerID)
);

-- ===========================
-- Create Stored Procedures
-- ===========================

-- 1. Add a New Author
ALTER PROCEDURE AddAuthor
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @DateOfBirth DATE
AS
BEGIN
    INSERT INTO Authors (FirstName, LastName, DateOfBirth)
    VALUES (@FirstName, @LastName, @DateOfBirth);
END;
GO

--- 2. Add a New Book
ALTER PROCEDURE AddBook
    @Title NVARCHAR(255),
    @AuthorID INT,
    @Genre NVARCHAR(50),
    @PublicationYear INT
AS
BEGIN
    -- Check if AuthorID exists in the Authors table
    IF NOT EXISTS (SELECT 1 FROM Authors WHERE AuthorID = @AuthorID)
    BEGIN
        PRINT 'Author does not exist.';
        RETURN; -- Exit the procedure if AuthorID does not exist
    END
    
    -- Insert the new book if the AuthorID is valid
    INSERT INTO Books (Title, AuthorID, Genre, PublicationYear)
    VALUES (@Title, @AuthorID, @Genre, @PublicationYear);

    PRINT 'Book added successfully.';
END;
GO



-- 3. Add a New Borrower
CREATE PROCEDURE AddBorrower
    @Name NVARCHAR(100),
    @Email NVARCHAR(100),
    @ContactNumber NVARCHAR(15)
AS
BEGIN
    INSERT INTO Borrowers (Name, Email, ContactNumber)
    VALUES (@Name, @Email, @ContactNumber);
END;
GO

-- 4. Borrow a Book
CREATE PROCEDURE BorrowBook
    @BookID INT,
    @BorrowerID INT,
    @BorrowDate DATE
AS
BEGIN
    DECLARE @Available INT;
    
    -- Check if the book is available for borrowing
    SELECT @Available = COUNT(*) FROM BorrowingRecords
    WHERE BookID = @BookID AND ReturnDate IS NULL;
    
    IF @Available > 0
    BEGIN
        PRINT 'Book is currently borrowed by someone else';
    END
    ELSE
    BEGIN
        -- Record the borrowing
        INSERT INTO BorrowingRecords (BookID, BorrowerID, BorrowDate)
        VALUES (@BookID, @BorrowerID, @BorrowDate);
    END
END;
GO

-- 5. Return a Book
CREATE PROCEDURE ReturnBook
    @BookID INT,
    @BorrowerID INT,
    @ReturnDate DATE
AS
BEGIN
    UPDATE BorrowingRecords
    SET ReturnDate = @ReturnDate
    WHERE BookID = @BookID AND BorrowerID = @BorrowerID AND ReturnDate IS NULL;
END;
GO

-- 6. Get All Books
CREATE PROCEDURE GetAllBooks
AS
BEGIN
    SELECT b.BookID, b.Title, a.FirstName + ' ' + a.LastName AS Author, b.Genre, b.PublicationYear
    FROM Books b
    INNER JOIN Authors a ON b.AuthorID = a.AuthorID;
END;
GO

-- 7. Get Book by ID
CREATE PROCEDURE GetBookByID
    @BookID INT
AS
BEGIN
    SELECT b.BookID, b.Title, a.FirstName + ' ' + a.LastName AS Author, b.Genre, b.PublicationYear
    FROM Books b
    INNER JOIN Authors a ON b.AuthorID = a.AuthorID
    WHERE b.BookID = @BookID;
END;
GO

-- 8. Update Book Information
CREATE PROCEDURE UpdateBook
    @BookID INT,
    @Title NVARCHAR(255),
    @AuthorID INT,
    @Genre NVARCHAR(50),
    @PublicationYear INT
AS
BEGIN
    UPDATE Books
    SET Title = @Title,
        AuthorID = @AuthorID,
        Genre = @Genre,
        PublicationYear = @PublicationYear
    WHERE BookID = @BookID;
END;
GO

-- 9. Delete Book
CREATE PROCEDURE DeleteBook
    @BookID INT
AS
BEGIN
    DELETE FROM Books WHERE BookID = @BookID;
END;
GO

-- 10. View Borrowing Records
CREATE PROCEDURE ViewBorrowingRecords
AS
BEGIN
    SELECT br.BorrowingRecordID, b.Title, br.BorrowDate, br.ReturnDate, bor.Name AS BorrowerName
    FROM BorrowingRecords br
    INNER JOIN Books b ON br.BookID = b.BookID
    INNER JOIN Borrowers bor ON br.BorrowerID = bor.BorrowerID;
END;
GO
