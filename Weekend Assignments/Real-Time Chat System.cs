using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

public class ChatMessage
{
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string MessageText { get; set; }
    public DateTime SentAt { get; set; }
}

public class ChatService
{
    private readonly string _connectionString;

    public ChatService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SaveMessageAsync(ChatMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        if (string.IsNullOrWhiteSpace(message.MessageText))
            throw new ArgumentException("Message text cannot be empty", nameof(message.MessageText));

        if (message.SenderId <= 0)
            throw new ArgumentException("Sender ID must be positive", nameof(message.SenderId));

        if (message.ReceiverId <= 0)
            throw new ArgumentException("Receiver ID must be positive", nameof(message.ReceiverId));

        if (message.SenderId == message.ReceiverId)
            throw new ArgumentException("Sender and Receiver cannot be the same");

        const string sql = @"
            INSERT INTO ChatMessages (SenderId, ReceiverId, MessageText, SentAt)
            VALUES (@SenderId, @ReceiverId, @MessageText, SYSDATETIME());
        ";

        try
        {
            await using SqlConnection conn = new(_connectionString);
            await using SqlCommand cmd = new(sql, conn);

            cmd.Parameters.Add("@SenderId", SqlDbType.Int).Value = message.SenderId;
            cmd.Parameters.Add("@ReceiverId", SqlDbType.Int).Value = message.ReceiverId;
            cmd.Parameters.Add("@MessageText", SqlDbType.NVarChar, 1000).Value = message.MessageText;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
        catch (SqlException ex)
        {
            throw new ApplicationException("Failed to save message to database", ex);
        }
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(int receiverId)
    {
        if (receiverId <= 0)
            throw new ArgumentException("Receiver ID must be positive", nameof(receiverId));

        const string sql = @"
            SELECT SenderId, ReceiverId, MessageText, SentAt
            FROM ChatMessages
            WHERE ReceiverId = @ReceiverId
            ORDER BY SentAt ASC;
        ";

        List<ChatMessage> messages = new();

        try
        {
            await using SqlConnection conn = new(_connectionString);
            await using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.Add("@ReceiverId", SqlDbType.Int).Value = receiverId;

            await conn.OpenAsync();
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                messages.Add(new ChatMessage
                {
                    SenderId = reader.GetInt32(0),
                    ReceiverId = reader.GetInt32(1),
                    MessageText = reader.GetString(2),
                    SentAt = reader.GetDateTime(3)
                });
            }
        }
        catch (SqlException ex)
        {
            throw new ApplicationException("Failed to retrieve messages from database", ex);
        }

        return messages;
    }

    public async Task<List<ChatMessage>> GetConversationAsync(int userId1, int userId2)
    {
        if (userId1 <= 0 || userId2 <= 0)
            throw new ArgumentException("User IDs must be positive");

        if (userId1 == userId2)
            throw new ArgumentException("User IDs must be different");

        const string sql = @"
            SELECT SenderId, ReceiverId, MessageText, SentAt
            FROM ChatMessages
            WHERE (SenderId = @UserId1 AND ReceiverId = @UserId2)
               OR (SenderId = @UserId2 AND ReceiverId = @UserId1)
            ORDER BY SentAt ASC;
        ";

        List<ChatMessage> messages = new();

        try
        {
            await using SqlConnection conn = new(_connectionString);
            await using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.Add("@UserId1", SqlDbType.Int).Value = userId1;
            cmd.Parameters.Add("@UserId2", SqlDbType.Int).Value = userId2;

            await conn.OpenAsync();
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                messages.Add(new ChatMessage
                {
                    SenderId = reader.GetInt32(0),
                    ReceiverId = reader.GetInt32(1),
                    MessageText = reader.GetString(2),
                    SentAt = reader.GetDateTime(3)
                });
            }
        }
        catch (SqlException ex)
        {
            throw new ApplicationException("Failed to retrieve conversation from database", ex);
        }

        return messages;
    }

    public async Task ClearChatAsync(int userId1, int userId2)
    {
        if (userId1 <= 0 || userId2 <= 0)
            throw new ArgumentException("User IDs must be positive");

        if (userId1 == userId2)
            throw new ArgumentException("User IDs must be different");

        const string sql = @"
            DELETE FROM ChatMessages
            WHERE (SenderId = @UserId1 AND ReceiverId = @UserId2)
               OR (SenderId = @UserId2 AND ReceiverId = @UserId1);
        ";

        try
        {
            await using SqlConnection conn = new(_connectionString);
            await using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.Add("@UserId1", SqlDbType.Int).Value = userId1;
            cmd.Parameters.Add("@UserId2", SqlDbType.Int).Value = userId2;

            await conn.OpenAsync();
            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                throw new ApplicationException("No messages found to delete");
            }
        }
        catch (SqlException ex)
        {
            throw new ApplicationException("Failed to clear chat messages", ex);
        }
    }

    public async Task ClearAllMessagesForUserAsync(int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be positive");

        const string sql = @"
            DELETE FROM ChatMessages
            WHERE SenderId = @UserId OR ReceiverId = @UserId;
        ";

        try
        {
            await using SqlConnection conn = new(_connectionString);
            await using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            await conn.OpenAsync();
            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                throw new ApplicationException("No messages found to delete");
            }
        }
        catch (SqlException ex)
        {
            throw new ApplicationException("Failed to clear user messages", ex);
        }
    }
}

class Program
{
    private const string ConnectionString = "Server=BISSU\\SQLEXPRESS;Database=ChatDB;Trusted_Connection=True;TrustServerCertificate=True;";
    private static readonly ChatService _chatService = new(ConnectionString);

    static void ClearAndPrintHeader()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════╗");
        Console.WriteLine("║             CHAT APPLICATION                   ║");
        Console.WriteLine("╚════════════════════════════════════════════════╝");
        Console.ResetColor();
    }

    static void DisplayMainMenu()
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("\n┌──────────────────────────────────────────────┐");
        Console.WriteLine("│                   MAIN MENU                  │");
        Console.WriteLine("├───────────┬──────────────────────────────────┤");
        Console.WriteLine("│   Option  │            Description           │");
        Console.WriteLine("├───────────┼──────────────────────────────────┤");
        Console.WriteLine("│     1     │ Send Message                     │");
        Console.WriteLine("│     2     │ View Received Messages           │");
        Console.WriteLine("│     3     │ View Conversation                │");
        Console.WriteLine("│     4     │ Clear Conversation               │");
        Console.WriteLine("│     5     │ Clear All Your Messages          │");
        Console.WriteLine("│     6     │ Exit                             │");
        Console.WriteLine("└───────────┴──────────────────────────────────┘");
        Console.ResetColor();
        Console.Write("\nEnter your choice (1-6): ");
    }

    static int GetMenuChoice()
    {
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= 6)
            {
                return choice;
            }
            Console.Write("Invalid input. Please enter a number between 1 and 6: ");
        }
    }

    static int GetUserId(string prompt)
    {
        Console.Write(prompt);
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int id) && id > 0)
            {
                return id;
            }
            Console.Write("Invalid user ID. Please enter a positive number: ");
        }
    }

    static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
    }

    static void WaitForAnyKey(string message = "Press any key to continue...")
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"\n{message}");
        Console.ResetColor();
        Console.ReadKey();
    }

    static void DisplayError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nError: {message}");
        Console.ResetColor();
        WaitForAnyKey();
    }

    static async Task SendMessage()
    {
        ClearAndPrintHeader();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════╗");
        Console.WriteLine("║               SEND MESSAGE                     ║");
        Console.WriteLine("╚════════════════════════════════════════════════╝");
        Console.ResetColor();

        int senderId = GetUserId("\nEnter Sender ID: ");
        int receiverId = GetUserId("Enter Receiver ID: ");

        if (senderId == receiverId)
        {
            throw new ApplicationException("Sender and Receiver cannot be the same");
        }

        Console.Write("Enter Message: ");
        string messageText = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(messageText))
        {
            throw new ApplicationException("Message cannot be empty");
        }

        await _chatService.SaveMessageAsync(new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            MessageText = messageText
        });

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Message sent successfully!");
        Console.ResetColor();
        WaitForAnyKey();
    }

    static async Task ViewMessages()
    {
        ClearAndPrintHeader();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════╗");
        Console.WriteLine("║           RECEIVED MESSAGES                    ║");
        Console.WriteLine("╚════════════════════════════════════════════════╝");
        Console.ResetColor();

        int receiverId = GetUserId("\nEnter your User ID to view messages: ");
        var messages = await _chatService.GetMessagesAsync(receiverId);

        if (messages.Count == 0)
        {
            Console.WriteLine("\nNo messages found.");
            WaitForAnyKey();
            return;
        }

        Console.WriteLine($"\nYou have {messages.Count} message(s):\n");
        Console.WriteLine("┌──────────┬──────────┬─────────────────────────────┐");
        Console.WriteLine("│   From   │   Time   │          Message            │");
        Console.WriteLine("├──────────┼──────────┼─────────────────────────────┤");

        foreach (var m in messages)
        {
            Console.WriteLine($"│ {m.SenderId,-8} │ {m.SentAt:HH:mm:ss} │ {Truncate(m.MessageText, 25),-25}   │");
        }
        Console.WriteLine("└──────────┴──────────┴─────────────────────────────┘");
        WaitForAnyKey();
    }

    static async Task ViewConversation()
    {
        ClearAndPrintHeader();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════╗");
        Console.WriteLine("║              CONVERSATION                      ║");
        Console.WriteLine("╚════════════════════════════════════════════════╝");
        Console.ResetColor();

        int userId1 = GetUserId("\nEnter first User ID: ");
        int userId2 = GetUserId("Enter second User ID: ");

        if (userId1 == userId2)
        {
            throw new ApplicationException("Cannot view conversation with yourself");
        }

        var messages = await _chatService.GetConversationAsync(userId1, userId2);

        if (messages.Count == 0)
        {
            Console.WriteLine("\nNo messages found between these users.");
            WaitForAnyKey();
            return;
        }

        Console.WriteLine($"\nConversation between {userId1} and {userId2} ({messages.Count} messages):\n");

        foreach (var m in messages)
        {
            if (m.SenderId == userId1)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"[{m.SentAt:HH:mm:ss}] {m.SenderId} -> {m.ReceiverId}: {m.MessageText}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{m.SentAt:HH:mm:ss}] {m.SenderId} -> {m.ReceiverId}: {m.MessageText}");
            }
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\n(End of conversation)");
        Console.ResetColor();
        WaitForAnyKey();
    }

    static async Task ClearConversation()
    {
        ClearAndPrintHeader();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════╗");
        Console.WriteLine("║           CLEAR CONVERSATION                   ║");
        Console.WriteLine("╚════════════════════════════════════════════════╝");
        Console.ResetColor();

        int userId1 = GetUserId("\nEnter your User ID: ");
        int userId2 = GetUserId("Enter other User ID to clear conversation with: ");

        if (userId1 == userId2)
        {
            throw new ApplicationException("Cannot clear conversation with yourself");
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("\nAre you sure you want to delete ALL messages between you and this user? (y/n): ");
        Console.ResetColor();
        var confirm = Console.ReadLine();

        if (confirm?.ToLower() != "y")
        {
            Console.WriteLine("Operation cancelled.");
            WaitForAnyKey();
            return;
        }

        try
        {
            await _chatService.ClearChatAsync(userId1, userId2);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✓ Conversation cleared successfully!");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            DisplayError(ex.Message);
            return;
        }

        WaitForAnyKey();
    }

    static async Task ClearAllUserMessages()
    {
        ClearAndPrintHeader();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════╗");
        Console.WriteLine("║         CLEAR ALL YOUR MESSAGES                ║");
        Console.WriteLine("╚════════════════════════════════════════════════╝");
        Console.ResetColor();

        int userId = GetUserId("\nEnter your User ID: ");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("\nWARNING: This will delete ALL messages you've sent AND received. Continue? (y/n): ");
        Console.ResetColor();
        var confirm = Console.ReadLine();

        if (confirm?.ToLower() != "y")
        {
            Console.WriteLine("Operation cancelled.");
            WaitForAnyKey();
            return;
        }

        try
        {
            await _chatService.ClearAllMessagesForUserAsync(userId);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✓ All your messages cleared successfully!");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            DisplayError(ex.Message);
            return;
        }

        WaitForAnyKey();
    }

    static async Task Main()
    {
        Console.Title = "Chat Application";

        while (true)
        {
            try
            {
                ClearAndPrintHeader();
                DisplayMainMenu();
                var choice = GetMenuChoice();

                switch (choice)
                {
                    case 1:
                        await SendMessage();
                        break;
                    case 2:
                        await ViewMessages();
                        break;
                    case 3:
                        await ViewConversation();
                        break;
                    case 4:
                        await ClearConversation();
                        break;
                    case 5:
                        await ClearAllUserMessages();
                        break;
                    case 6:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\nExiting application...");
                        Console.ResetColor();
                        return;
                }
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }
        }
    }
}