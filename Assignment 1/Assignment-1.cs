using System;
using System.Data;
using System.Data.SqlClient;

namespace EmployeeManagementSystem
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }
    }

    public class EmployeeRepository
    {
        // Update this connection string according to your environment
        private string connectionString = @"Server=BISSU\SQLEXPRESS;Database=EmployeeDB;Integrated Security=True;";

        public void AddEmployee(Employee employee)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO Employees 
                                   (FirstName, LastName, Email, Phone, Department, Position, Salary) 
                                   VALUES 
                                   (@FirstName, @LastName, @Email, @Phone, @Department, @Position, @Salary)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                        command.Parameters.AddWithValue("@LastName", employee.LastName);
                        command.Parameters.AddWithValue("@Email", employee.Email ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Phone", employee.Phone ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Department", employee.Department ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Position", employee.Position ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Salary", employee.Salary);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding employee: {ex.Message}");
                throw;
            }
        }

        public DataTable GetAllEmployees()
        {
            DataTable employees = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Employees";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            employees.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving employees: {ex.Message}");
                throw;
            }

            return employees;
        }

        public void UpdateEmployee(Employee employee)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE Employees SET 
                                   FirstName = @FirstName, 
                                   LastName = @LastName, 
                                   Email = @Email, 
                                   Phone = @Phone, 
                                   Department = @Department, 
                                   Position = @Position, 
                                   Salary = @Salary 
                                   WHERE EmployeeID = @EmployeeID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EmployeeID", employee.EmployeeID);
                        command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                        command.Parameters.AddWithValue("@LastName", employee.LastName);
                        command.Parameters.AddWithValue("@Email", employee.Email ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Phone", employee.Phone ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Department", employee.Department ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Position", employee.Position ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Salary", employee.Salary);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            throw new Exception("No employee found with the specified ID.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating employee: {ex.Message}");
                throw;
            }
        }

        public void DeleteEmployee(int employeeId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EmployeeID", employeeId);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            throw new Exception("No employee found with the specified ID.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting employee: {ex.Message}");
                throw;
            }
        }
    }

    class Program
    {
        static EmployeeRepository employeeRepo = new EmployeeRepository();

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Employee Management System");
                Console.WriteLine("-------------------------");

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("\nMAIN MENU");
                    Console.WriteLine("1. Add Employee");
                    Console.WriteLine("2. View All Employees");
                    Console.WriteLine("3. Update Employee");
                    Console.WriteLine("4. Delete Employee");
                    Console.WriteLine("5. Exit");
                    Console.Write("Select an option (1-5): ");

                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            Console.Clear();
                            AddEmployee();
                            Console.WriteLine("\nPress any key to continue...");
                            Console.ReadKey();
                            break;

                        case "2":
                            Console.Clear();
                            ViewAllEmployees();
                            Console.WriteLine("\nPress any key to continue...");
                            Console.ReadKey();
                            break;

                        case "3":
                            Console.Clear();
                            UpdateEmployee();
                            Console.WriteLine("\nPress any key to continue...");
                            Console.ReadKey();
                            break;

                        case "4":
                            Console.Clear();
                            DeleteEmployee();
                            Console.WriteLine("\nPress any key to continue...");
                            Console.ReadKey();
                            break;

                        case "5":
                            Console.Clear();
                            Console.WriteLine("Exiting program...");
                            Environment.Exit(0);
                            break;

                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            Console.WriteLine("\nPress any key to continue...");
                            Console.ReadKey();
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nERROR: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        static void AddEmployee()
        {
            Console.WriteLine("\nADD NEW EMPLOYEE");
            Console.WriteLine("----------------");

            Employee emp = new Employee();

            Console.Write("First Name: ");
            emp.FirstName = Console.ReadLine();

            Console.Write("Last Name: ");
            emp.LastName = Console.ReadLine();

            Console.Write("Email: ");
            emp.Email = Console.ReadLine();

            Console.Write("Phone: ");
            emp.Phone = Console.ReadLine();

            Console.Write("Department: ");
            emp.Department = Console.ReadLine();

            Console.Write("Position: ");
            emp.Position = Console.ReadLine();

            Console.Write("Salary: ");
            decimal salaryInput;
            while (!decimal.TryParse(Console.ReadLine(), out salaryInput))
            {
                Console.Write("Invalid input. Please enter a valid salary: ");
            }
            emp.Salary = salaryInput;



            try
            {
                employeeRepo.AddEmployee(emp);
                Console.WriteLine("\nEmployee added successfully!");
            }
            catch
            {
                Console.WriteLine("\nFailed to add employee.");
            }
        }

        static void ViewAllEmployees()
        {
            Console.WriteLine("\nEMPLOYEE LIST");
            Console.WriteLine("-------------");

            try
            {
                DataTable employees = employeeRepo.GetAllEmployees();

                if (employees.Rows.Count == 0)
                {
                    Console.WriteLine("No employees found in the database.");
                    return;
                }

                foreach (DataRow row in employees.Rows)
                {
                    Console.WriteLine($"ID: {row["EmployeeID"]}");
                    Console.WriteLine($"Name: {row["FirstName"]} {row["LastName"]}");
                    Console.WriteLine($"Email: {row["Email"]}");
                    Console.WriteLine($"Phone: {row["Phone"]}");
                    Console.WriteLine($"Department: {row["Department"]}");
                    Console.WriteLine($"Position: {row["Position"]}");
                    Console.WriteLine($"Salary: {row["Salary"]:C}");
                    Console.WriteLine($"Hire Date: {Convert.ToDateTime(row["HireDate"]).ToShortDateString()}");
                    Console.WriteLine("-----------------------------");
                }
            }
            catch
            {
                Console.WriteLine("Failed to retrieve employee data.");
            }
        }

        static void UpdateEmployee()
        {
            Console.WriteLine("\nUPDATE EMPLOYEE");
            Console.WriteLine("--------------");

            Console.Write("Enter Employee ID to update: ");
            int id;
            while (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.Write("Invalid input. Please enter a valid ID: ");
            }

            try
            {
                DataTable employees = employeeRepo.GetAllEmployees();
                DataRow[] foundRows = employees.Select($"EmployeeID = {id}");

                if (foundRows.Length == 0)
                {
                    Console.WriteLine("Employee not found.");
                    return;
                }

                DataRow row = foundRows[0];
                Employee emp = new Employee()
                {
                    EmployeeID = id,
                    FirstName = row["FirstName"].ToString(),
                    LastName = row["LastName"].ToString(),
                    Email = row["Email"].ToString(),
                    Phone = row["Phone"].ToString(),
                    Department = row["Department"].ToString(),
                    Position = row["Position"].ToString(),
                    Salary = Convert.ToDecimal(row["Salary"]),
                    HireDate = Convert.ToDateTime(row["HireDate"])
                };

                Console.WriteLine("\nCurrent Details:");
                Console.WriteLine($"1. First Name: {emp.FirstName}");
                Console.WriteLine($"2. Last Name: {emp.LastName}");
                Console.WriteLine($"3. Email: {emp.Email}");
                Console.WriteLine($"4. Phone: {emp.Phone}");
                Console.WriteLine($"5. Department: {emp.Department}");
                Console.WriteLine($"6. Position: {emp.Position}");
                Console.WriteLine($"7. Salary: {emp.Salary:C}");

                Console.WriteLine("\nEnter new values (press Enter to keep current value):");

                Console.Write("First Name: ");
                string input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) emp.FirstName = input;

                Console.Write("Last Name: ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) emp.LastName = input;

                Console.Write("Email: ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) emp.Email = input;

                Console.Write("Phone: ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) emp.Phone = input;

                Console.Write("Department: ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) emp.Department = input;

                Console.Write("Position: ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) emp.Position = input;

                Console.Write("Salary: ");
                input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input) && decimal.TryParse(input, out decimal newSalary))
                {
                    emp.Salary = newSalary;
                }

                employeeRepo.UpdateEmployee(emp);
                Console.WriteLine("\nEmployee updated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError updating employee: {ex.Message}");
            }
        }

        static void DeleteEmployee()
        {
            Console.WriteLine("\nDELETE EMPLOYEE");
            Console.WriteLine("--------------");

            Console.Write("Enter Employee ID to delete: ");
            int id;
            while (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.Write("Invalid input. Please enter a valid ID: ");
            }

            Console.Write("Are you sure you want to delete this employee? (y/n): ");
            if (Console.ReadLine().ToLower() != "y")
            {
                Console.WriteLine("Deletion cancelled.");
                return;
            }

            try
            {
                employeeRepo.DeleteEmployee(id);
                Console.WriteLine("\nEmployee deleted successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError deleting employee: {ex.Message}");
            }
        }
    }
}