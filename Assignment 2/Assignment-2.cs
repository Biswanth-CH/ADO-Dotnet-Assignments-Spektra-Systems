using System;
using System.Data;
using System.Data.SqlClient;

class LibraryManagementSystem
{
    private static string connectionString = @"Server=BISSU\SQLEXPRESS;Database=LibraryDB;Integrated Security=True;";

    public static void Main()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("\n=== Library Management System ===");
            Console.WriteLine("1. Add Author");
            Console.WriteLine("2. View Authors");
            Console.WriteLine("3. Add Book");
            Console.WriteLine("4. View Books");
            Console.WriteLine("5. Add Borrower");
            Console.WriteLine("6. View Borrowers");
            Console.WriteLine("7. Borrow Book");
            Console.WriteLine("8. Return Book");
            Console.WriteLine("9. View Borrowing Records");
            Console.WriteLine("10. Exit");
            Console.Write("\nEnter your choice: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddAuthor();
                    Console.ReadKey();
                    break;
                case "2":
                    ViewAuthors();
                    Console.ReadKey();
                    break;
                case "3":
                    AddBook();
                    Console.ReadKey();
                    break;
                case "4":
                    ViewBooks();
                    Console.ReadKey();
                    break;
                case "5":
                    AddBorrower();
                    Console.ReadKey();
                    break;
                case "6":
                    ViewBorrowers();
                    Console.ReadKey();
                    break;
                case "7":
                    BorrowBook();
                    Console.ReadKey();
                    break;
                case "8":
                    ReturnBook();
                    Console.ReadKey();
                    break;
                case "9":
                    ViewBorrowingRecords();
                    Console.ReadKey();
                    break;
                case "10":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid choice, please try again.");
                    break;
            }
        }
    }

    #region Author Methods
    public static void AddAuthor()
    {
        Console.WriteLine("\n--- Add New Author ---");
        Console.Write("First Name: ");
        string firstName = Console.ReadLine();

        Console.Write("Last Name: ");
        string lastName = Console.ReadLine();

        Console.Write("Date of Birth (YYYY-MM-DD): ");
        DateTime dateOfBirth;
        while (!DateTime.TryParse(Console.ReadLine(), out dateOfBirth))
        {
            Console.Write("Invalid Date format. Please enter a valid date (YYYY-MM-DD): ");
        }

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("AddAuthor", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@FirstName", firstName);
            cmd.Parameters.AddWithValue("@LastName", lastName);
            cmd.Parameters.AddWithValue("@DateOfBirth", dateOfBirth);

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                Console.WriteLine("\nAuthor added successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
        }
    }

    public static void ViewAuthors()
    {
        Console.WriteLine("\n--- Author List ---");
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("SELECT AuthorID, FirstName, LastName, DateOfBirth FROM Authors", conn);

            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("No authors found.");
                    return;
                }

                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["AuthorID"]}, Name: {reader["FirstName"]} {reader["LastName"]}, DOB: {reader["DateOfBirth"]:yyyy-MM-dd}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
    #endregion

    #region Book Methods
    public static void AddBook()
    {
        Console.WriteLine("\n--- Add New Book ---");

        // Show available authors
        ViewAuthors();

        Console.Write("\nEnter Book Title: ");
        string title = Console.ReadLine();

        Console.Write("Enter Author ID (or 0 to add new author): ");
        int authorId;
        while (!int.TryParse(Console.ReadLine(), out authorId) || authorId < 0)
        {
            Console.Write("Invalid Author ID. Please enter a valid positive integer or 0: ");
        }

        if (authorId == 0)
        {
            AddAuthor();
            // Get the newly created author's ID
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT TOP 1 AuthorID FROM Authors ORDER BY AuthorID DESC", conn);
                try
                {
                    conn.Open();
                    authorId = Convert.ToInt32(cmd.ExecuteScalar());
                    Console.WriteLine($"Using new author ID: {authorId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting new author ID: {ex.Message}");
                    return;
                }
            }
        }

        Console.Write("Enter Genre: ");
        string genre = Console.ReadLine();

        Console.Write("Enter Publication Year: ");
        int publicationYear;
        while (!int.TryParse(Console.ReadLine(), out publicationYear) || publicationYear <= 0)
        {
            Console.Write("Invalid Publication Year. Please enter a valid year: ");
        }

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("AddBook", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@AuthorID", authorId);
            cmd.Parameters.AddWithValue("@Genre", genre);
            cmd.Parameters.AddWithValue("@PublicationYear", publicationYear);

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                Console.WriteLine("\nBook added successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
        }
    }

    public static void ViewBooks()
    {
        Console.WriteLine("\n--- Book List ---");
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("GetAllBooks", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("No books found.");
                    return;
                }

                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["BookID"]}, Title: {reader["Title"]}, Author: {reader["Author"]}, Genre: {reader["Genre"]}, Year: {reader["PublicationYear"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
    #endregion

    #region Borrower Methods
    public static void AddBorrower()
    {
        Console.WriteLine("\n--- Add New Borrower ---");
        Console.Write("Name: ");
        string name = Console.ReadLine();

        Console.Write("Email: ");
        string email = Console.ReadLine();

        Console.Write("Contact Number: ");
        string contactNumber = Console.ReadLine();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("AddBorrower", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@ContactNumber", contactNumber);

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                Console.WriteLine("\nBorrower added successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
        }
    }

    public static void ViewBorrowers()
    {
        Console.WriteLine("\n--- Borrower List ---");
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("SELECT BorrowerID, Name, Email, ContactNumber FROM Borrowers", conn);

            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("No borrowers found.");
                    return;
                }

                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["BorrowerID"]}, Name: {reader["Name"]}, Email: {reader["Email"]}, Phone: {reader["ContactNumber"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
    #endregion

    #region Borrowing Methods
    public static void BorrowBook()
    {
        Console.WriteLine("\n--- Borrow a Book ---");

        // Show available books
        ViewBooks();

        Console.Write("\nEnter Book ID to borrow: ");
        int bookId;
        while (!int.TryParse(Console.ReadLine(), out bookId) || bookId <= 0)
        {
            Console.Write("Invalid Book ID. Please enter a valid Book ID: ");
        }

        // Show available borrowers
        ViewBorrowers();

        Console.Write("\nEnter Borrower ID: ");
        int borrowerId;
        while (!int.TryParse(Console.ReadLine(), out borrowerId) || borrowerId <= 0)
        {
            Console.Write("Invalid Borrower ID. Please enter a valid Borrower ID: ");
        }

        Console.Write("Enter Borrow Date (YYYY-MM-DD): ");
        DateTime borrowDate;
        while (!DateTime.TryParse(Console.ReadLine(), out borrowDate))
        {
            Console.Write("Invalid Date format. Please enter a valid date (YYYY-MM-DD): ");
        }

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("BorrowBook", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@BookID", bookId);
            cmd.Parameters.AddWithValue("@BorrowerID", borrowerId);
            cmd.Parameters.AddWithValue("@BorrowDate", borrowDate);

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                Console.WriteLine("\nBook borrowed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
        }
    }

    public static void ReturnBook()
    {
        Console.WriteLine("\n--- Return a Book ---");

        // Show active borrowing records
        ViewBorrowingRecords();

        Console.Write("\nEnter Book ID to return: ");
        int bookId;
        while (!int.TryParse(Console.ReadLine(), out bookId) || bookId <= 0)
        {
            Console.Write("Invalid Book ID. Please enter a valid Book ID: ");
        }

        Console.Write("Enter Borrower ID: ");
        int borrowerId;
        while (!int.TryParse(Console.ReadLine(), out borrowerId) || borrowerId <= 0)
        {
            Console.Write("Invalid Borrower ID. Please enter a valid Borrower ID: ");
        }

        Console.Write("Enter Return Date (YYYY-MM-DD): ");
        DateTime returnDate;
        while (!DateTime.TryParse(Console.ReadLine(), out returnDate))
        {
            Console.Write("Invalid Date format. Please enter a valid date (YYYY-MM-DD): ");
        }

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("ReturnBook", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@BookID", bookId);
            cmd.Parameters.AddWithValue("@BorrowerID", borrowerId);
            cmd.Parameters.AddWithValue("@ReturnDate", returnDate);

            try
            {
                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Console.WriteLine("\nBook returned successfully!");
                }
                else
                {
                    Console.WriteLine("\nNo matching borrowing record found or book was already returned.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
        }
    }

    public static void ViewBorrowingRecords()
    {
        Console.WriteLine("\n--- Borrowing Records ---");
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("ViewBorrowingRecords", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("No borrowing records found.");
                    return;
                }

                while (reader.Read())
                {
                    string returnStatus = string.IsNullOrEmpty(reader["ReturnDate"].ToString())
                        ? "Not Returned"
                        : $"Returned on: {Convert.ToDateTime(reader["ReturnDate"]):yyyy-MM-dd}";

                    Console.WriteLine($"Record ID: {reader["BorrowingRecordID"]}, " +
                                    $"Book: {reader["Title"]}, " +
                                    $"Borrower: {reader["BorrowerName"]}, " +
                                    $"Borrowed: {Convert.ToDateTime(reader["BorrowDate"]):yyyy-MM-dd}, " +
                                    returnStatus);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
    #endregion
}