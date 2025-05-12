		CREATE DATABASE EmployeeDB;

		USE EmployeeDB;

		CREATE TABLE Employees (
			EmployeeID INT PRIMARY KEY IDENTITY(1,1),
			FirstName NVARCHAR(50) NOT NULL,
			LastName NVARCHAR(50) NOT NULL,
			Email NVARCHAR(100) UNIQUE,
			Phone NVARCHAR(20),
			Department NVARCHAR(50),
			Position NVARCHAR(50),
			Salary DECIMAL(10,2),
			HireDate DATE DEFAULT GETDATE()
		);

