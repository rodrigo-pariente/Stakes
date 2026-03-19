using Microsoft.Data.Sqlite;


class Database
{
    public static SqliteConnection GetConnection()
    {
        var connection = new SqliteConnection("Data source=stakes.db");
        connection.Open();

        return connection;
    }

    public static void Initialize()
    {
        using var connection = GetConnection();
        var emptyDict = new Dictionary<string, string>{};

        string createHabitsTable = """
            CREATE TABLE IF NOT EXISTS habits (
                id INTEGER PRIMARY KEY,
                name TEXT UNIQUE NOT NULL,
                hour_goal TEXT NOT NULL
            );
        """;
        ExecuteCommand(createHabitsTable, emptyDict);

        string createCommitsTable = """
            CREATE TABLE IF NOT EXISTS commits (
                id INTEGER PRIMARY KEY,
                habit_id INTEGER NOT NULL,
                effort_time TEXT NOT NULL,
                commit_time TEXT NOT NULL,
                message TEXT,
                FOREIGN KEY (habit_id)
                    REFERENCES habits (id)
                        ON UPDATE CASCADE
                        ON DELETE CASCADE
            );
        """;
        ExecuteCommand(createCommitsTable, emptyDict);

        string createQuotesTable = """
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
        ExecuteCommand(createQuotesTable, emptyDict);
    }

    public static void ExecuteCommand(
        string commandText,
        Dictionary<string, string> parameters // could have default value = emptyDict
    )
    {
        using var connection = GetConnection();

        var command = connection.CreateCommand();
        command.CommandText = commandText;

        foreach (KeyValuePair<string, string> pair in parameters)
        {
            command.Parameters.AddWithValue(pair.Key, pair.Value);
        }

        command.ExecuteNonQuery();
    }
}
