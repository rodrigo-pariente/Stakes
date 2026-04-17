using Stakes.Core;
using Stakes.Models;


class QuoteTable
{
    public static void Add(string habitName, string quote)
    {
        string command = """
            INSERT INTO quotes (habit_id, quote)
            VALUES (
                (SELECT id FROM habits WHERE name = $habit_name),
                $quote
            );
        """;

        var parameters = new Dictionary<string, string>{
            {"$habit_name", habitName},
            {"$quote", quote}
        };

        try
        {
            Database.ExecuteCommand(command, parameters);
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            throw new Exceptions.HabitNotFoundException(habitName);
        }
    }

    public static List<Quote> GetAll()
    {
        using var connection = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT quotes.id, habits.name, quote
            FROM quotes
            INNER JOIN habits
            ON habits.id = quotes.habit_id
        """;

        var quotes = new List<Quote>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var quote = new Quote(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2)
            );
            quotes.Add(quote);
        }

        return quotes;
    }

    public static Quote Get(int quoteId)
    {
        using var connection = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT quotes.id, habits.name, quote
            FROM quotes
            INNER JOIN habits
            ON habits.id = quotes.habit_id
            WHERE quotes.id = $quote_id
        """;
        command.Parameters.AddWithValue("$quote_id", quoteId.ToString());

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var quote = new Quote(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2)
            );
            return quote;
        }

        throw new Exceptions.QuoteNotFoundException(quoteId);
    }

    public static List<Quote> GetByHabit(string habitName)
    {
        using var connection = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT quotes.id, habits.name, quote
            FROM quotes
            INNER JOIN habits
            ON habits.id = quotes.habit_id
            WHERE habit_id = (SELECT id FROM habits WHERE name = $habit_name)
        """;

        command.Parameters.AddWithValue("$habit_name", habitName);

        var quotes = new List<Quote>();
        Quote quote;

        try
        {
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                quote = new Quote(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2)
                );
                quotes.Add(quote);
            }
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            throw new Exceptions.HabitNotFoundException(habitName);
        }
        return quotes;
    }

    public static void Remove(int quoteId)
    {
        string command = """
            DELETE FROM quotes
            WHERE id = $quote_id
        """;
        var parameters = new Dictionary<string, string>{
            {"$quote_id", quoteId.ToString()}
        };

        try
        {
            Database.ExecuteCommand(command, parameters);
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            throw new Exceptions.QuoteNotFoundException(quoteId);
        }
    }
}
