using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

public class Employee
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; }
    public string Department { get; set; }
    public DateTime JoinDate { get; set; }
    public string Email { get; set; }
}

public class EmployeeService
{
    private readonly string _connectionString;

    public EmployeeService(string connectionString)
    {
        _connectionString = connectionString; 
    }

    public List<Employee> GetRecentHiresSync()
    {
        const string sql = @"
            SELECT EmployeeId, FullName, Department, JoinDate, Email
            FROM Employees
            WHERE JoinDate >= DATEADD(MONTH, -6, GETDATE())
            ORDER BY JoinDate ASC;
        ";

        List<Employee> employees = new();

        using SqlConnection conn = new(_connectionString);
        using SqlCommand cmd = new(sql, conn);

        conn.Open();
        using SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            employees.Add(new Employee
            {
                EmployeeId = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Department = reader.IsDBNull(2) ? null : reader.GetString(2),
                JoinDate = reader.GetDateTime(3),
                Email = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }

        return employees;
    }

    public async Task<List<Employee>> GetRecentHiresAsync()
    {
        const string sql = @"
            SELECT EmployeeId, FullName, Department, JoinDate, Email
            FROM Employees
            WHERE JoinDate >= DATEADD(MONTH, -6, GETDATE())
            ORDER BY JoinDate ASC;
        ";

        List<Employee> employees = new();

        await using SqlConnection conn = new(_connectionString);
        await using SqlCommand cmd = new(sql, conn);

        await conn.OpenAsync();
        await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            employees.Add(new Employee
            {
                EmployeeId = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Department = reader.IsDBNull(2) ? null : reader.GetString(2),
                JoinDate = reader.GetDateTime(3),
                Email = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }

        return employees;
    }
}

class Program
{
    private const string ConnectionString = "Server=BISSU\\SQLEXPRESS;Database=TrafficDB;Trusted_Connection=True;TrustServerCertificate=True;";

    static async Task Main()
    {
        EmployeeService service = new(ConnectionString);
        bool exitRequested = false;

        while (!exitRequested)
        {
            Console.Clear();
            DrawHeader("EMPLOYEE MANAGEMENT SYSTEM");
            DrawMainMenu();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nEnter your choice (1-3): ");
            Console.ResetColor();
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Clear();
                    DrawHeader("SYNC MODE: FETCHING RECENT HIRES");
                    Stopwatch swSync = Stopwatch.StartNew();
                    List<Employee> syncEmployees = service.GetRecentHiresSync();
                    swSync.Stop();
                    PrintEmployees(syncEmployees);
                    ShowExecutionTime(swSync.ElapsedMilliseconds);
                    System.Threading.Thread.Sleep(500);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\n  Press any key to continue...");
                    Console.ResetColor();
                    Console.ReadKey();
                    break;

                case "2":
                    Console.Clear();
                    DrawHeader("ASYNC MODE: FETCHING RECENT HIRES");
                    Stopwatch swAsync = Stopwatch.StartNew();
                    List<Employee> asyncEmployees = await service.GetRecentHiresAsync();
                    swAsync.Stop();
                    PrintEmployees(asyncEmployees);
                    ShowExecutionTime(swAsync.ElapsedMilliseconds);
                    System.Threading.Thread.Sleep(500);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\n  Press any key to continue...");
                    Console.ResetColor();
                    Console.ReadKey();
                    break;

                case "3":
                    exitRequested = true;
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n  Invalid input. Please select 1, 2 or 3.");
                    Console.ResetColor();
                    Pause();
                    break;
            }
        }

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  Application exited successfully.");
        Console.ResetColor();
    }

    static void DrawMainMenu()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        string border = new string('═', 60);
        Console.WriteLine($"╔{border}╗");
        Console.WriteLine("║                MAIN MENU - OPTIONS                         ║");
        Console.WriteLine($"╠{border}╣");
        Console.WriteLine("║  1.  Retrieve Recent Hires (Synchronous)                   ║");
        Console.WriteLine("║  2.  Retrieve Recent Hires (Asynchronous)                  ║");
        Console.WriteLine("║  3.  Exit                                                  ║");
        Console.WriteLine($"╚{border}╝");
        Console.ResetColor();
    }

    static void PrintEmployees(List<Employee> employees)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  EMPLOYEE LIST");
        Console.ResetColor();

        string separator = new string('═', 101);
        Console.WriteLine($"╔{separator}╗");
        Console.WriteLine($"║ {"ID",-4} │ {"FULL NAME",-20} │ {"DEPARTMENT",-15} │ {"JOIN DATE",-14} │ {"EMAIL",-34} ║");
        Console.WriteLine($"╠{separator}╣");

        foreach (var emp in employees)
        {
            Console.WriteLine($"║ {emp.EmployeeId,-4} │ {emp.FullName,-20} │ {emp.Department,-15} │ {emp.JoinDate:yyyy-MM-dd,-12} │ {emp.Email,-34} ║");
        }

        Console.WriteLine($"╚{separator}╝");
    }

    static void Pause()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\n  Press any key to return to the main menu...");
        Console.ResetColor();
        Console.ReadKey(true);
    }

    static void ShowExecutionTime(long milliseconds)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n  Time taken: {milliseconds} ms");
        Console.ResetColor();
    }

    static void DrawHeader(string title)
    {
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine("=====================================================================");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"                      {title.ToUpper()}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine("=====================================================================\n");
        Console.ResetColor();
    }
}
