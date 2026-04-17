using Microsoft.Data.Sqlite;
using Stakes.Core;


class Database
{
    private static string? _databasePath;

    public static SqliteConnection GetConnection()
    {
        try
        {
            var connection = new SqliteConnection(
                $"Data source={_databasePath}"
            );
            connection.Open();

            return connection;
        }
        catch(UnauthorizedAccessException)
        {
            throw new Exceptions.UnauthorizedFileCreationException(
                "database file"
            );
        }
    }

    public static void ExecuteCommand(
        string commandText,
        Dictionary<string, string>? parameters = null
    )
    {
        parameters ??= [];

        using var connection = GetConnection();

        var command = connection.CreateCommand();
        command.CommandText = commandText;

        foreach (KeyValuePair<string, string> pair in parameters)
        {
            command.Parameters.AddWithValue(pair.Key, pair.Value);
        }

        command.ExecuteNonQuery();
    }

    public static void Initialize(string databasePath)
    {
        _databasePath = databasePath;

        using var connection = GetConnection();

        const string habitsTableDefinition = """
            CREATE TABLE IF NOT EXISTS habits (
                id INTEGER PRIMARY KEY,
                name TEXT UNIQUE NOT NULL,
                hour_goal TEXT NOT NULL DEFAULT "0",
                per_day_goal INTEGER NOT NULL DEFAULT 0,
                streak INTEGER NOT NULL DEFAULT 0
            );
        """;
        ExecuteCommand(habitsTableDefinition);

        const string commitsTableDefinition = """
            CREATE TABLE IF NOT EXISTS commits (
                id INTEGER PRIMARY KEY,
                habit_id INTEGER NOT NULL,
                time TEXT NOT NULL,
                date TEXT NOT NULL,
                message TEXT,
                FOREIGN KEY (habit_id)
                    REFERENCES habits (id)
                        ON UPDATE CASCADE
                        ON DELETE CASCADE
            );
        """;
        ExecuteCommand(commitsTableDefinition);

        const string quotesTableDefinition = """
            CREATE TABLE IF NOT EXISTS quotes (
                id INTEGER PRIMARY KEY,
                habit_id INTEGER NOT NULL,
                quote TEXT NOT NULL,
                FOREIGN KEY (habit_id)
                    REFERENCES habits (id)
                        ON UPDATE CASCADE
                        ON DELETE CASCADE
            );
        """;
        ExecuteCommand(quotesTableDefinition);
    }
}
