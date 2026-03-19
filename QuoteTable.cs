using Microsoft.Data.Sqlite;


class QuoteTable
{
    public record Quote(int Id, int HabitId, string Message);

    public static List<Quote> GetAll()
    {
        using var connection = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT *
            FROM quotes;
        """;

        var quotes = new List<Quote>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var quote = new Quote(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2)
            );
            quotes.Add(quote);
        }

        return quotes;
    }

    public static List<Quote> GetOfHabit(int habitId)
    {
        using var connection = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT *
            FROM quotes
            WHERE habit_id = $habit_id;
        """;

        command.Parameters.AddWithValue("$habit_id", habitId.ToString());

        var quotes = new List<Quote>();
        Quote quote;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            quote = new Quote(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2)
            );
            quotes.Add(quote);
        }
        // // c# may have better suiting exception
        // throw new IndexOutOfRangeException("No quote of given id!");
        return quotes;
    }

    public static void Remove(int quoteId)
    {
        string command = """
            DELETE FROM quotes
            WHERE id = $quote_id;
        """;
        var parameters = new Dictionary<string, string>{
            {"$quote_id", quoteId.ToString()}
        };

        Database.ExecuteCommand(command, parameters);
    }

    public static void Add(int habitId, string quote)
    {
        string command = """
            INSERT INTO quotes (habit_id, quote)
            VALUES ($habitId, $quote);
        """;

        var parameters = new Dictionary<string, string>{
            {"$habitId", habitId.ToString()},
            {"$quote", quote}
        };

        Database.ExecuteCommand(command, parameters);
    }
}
