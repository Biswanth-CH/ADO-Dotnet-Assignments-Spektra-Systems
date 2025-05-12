-- Create database if it doesn't exist

CREATE DATABASE TrafficDB;

USE TrafficDB;

-- Create Employees table
CREATE TABLE Employees (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    JoinDate DATETIME NOT NULL DEFAULT GETDATE(),
    Department NVARCHAR(50),
    Position NVARCHAR(50)
);
GO

-- Insert sample data
INSERT INTO Employees (FullName, Email, JoinDate, Department, Position)
VALUES
('Yaswanth Behara', 'yaswanth_behara@srmap.edu.in', DATEADD(MONTH, -2, GETDATE()), 'IT', 'Developer'),
('Niteesh Kumar Reddy', 'niteeshkumar_gajjala@srmap.edu.in', DATEADD(MONTH, -3, GETDATE()), 'HR', 'Recruiter'),
('Nandini Pesala', 'nandini_pesala@srmap.edu.in', DATEADD(MONTH, -1, GETDATE()), 'Finance', 'Accountant'),
('Iliaz Pthan', 'iliazkhan_pathan@srmap.edu.in', DATEADD(MONTH, -4, GETDATE()), 'IT', 'System Admin'),
('Dheeraj Koppaka', 'dheeraj_koppaka@srmap.edu.in', DATEADD(MONTH, -5, GETDATE()), 'Marketing', 'Analyst'),
('Biswanth Chevula', 'biswanth_chevula@srmap.edu.in', DATEADD(MONTH, -6, GETDATE()), 'HR', 'Manager'),
('Arshad Uzzama Shaik', 'uzzama_shaik@srmap.edu.in', DATEADD(MONTH, -7, GETDATE()), 'Finance', 'Manager'),
('Rithika Nampally', 'rithika_nampally@srmap.edu.in', DATEADD(MONTH, -1, GETDATE()), 'Marketing', 'Coordinator'),
('Talluri Nageswararao', 'nageswararao_t@srmap.edu.in', DATEADD(MONTH, -2, GETDATE()), 'IT', 'Developer'),
('Harshith Pavan Kumar', 'pavankumar_s@srmap.edu.in', DATEADD(MONTH, -3, GETDATE()), 'HR', 'Specialist'),
('Chodisetti Manikanta', 'manikanta_chodisetti@srmap.edu.in', DATEADD(MONTH, -4, GETDATE()), 'Finance', 'Analyst'),
('Lakkimsetty Sanjana', 'sanjana_l@srmap.edu.in', DATEADD(MONTH, -5, GETDATE()), 'Marketing', 'Designer');

-- Create index on JoinDate for performance

 CREATE INDEX IX_Employees_JoinDate ON Employees (JoinDate);

 drop table Employees;

