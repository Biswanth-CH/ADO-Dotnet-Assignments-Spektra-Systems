create database ChatDB;
use ChatDB;

-- Run this in SQL Server Management Studio

-- Step 1: Create Users table (optional but recommended)
CREATE TABLE Users (
    UserId     INT IDENTITY(1,1) PRIMARY KEY,
    Username   NVARCHAR(100) NOT NULL UNIQUE
);

-- Step 2: Create ChatMessages table
CREATE TABLE ChatMessages (
    MessageId     INT IDENTITY(1,1) PRIMARY KEY,
    SenderId      INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    ReceiverId    INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    MessageText   NVARCHAR(MAX) NOT NULL,
    SentAt        DATETIME2(3) DEFAULT SYSDATETIME()
);

-- Step 3: Performance index for faster message retrieval
CREATE NONCLUSTERED INDEX IX_ChatMessages_ReceiverId_SentAt
    ON ChatMessages (ReceiverId, SentAt DESC);

-- Step 4: Insert sample users for testing
INSERT INTO Users (Username) VALUES ('Biswanth'), ('Harshith'), ('Nageswar');

drop table Users;	
drop table ChatMessages;

select * from ChatMessages;