using Stakes.Core;
using Stakes.Models;


class CommitTable
{
    public static void Add(
        string habitName,
        string time,
        string date,
        string message
    )
    {
        string command = """
            INSERT INTO commits (
                habit_id,
                time,
                date,
                message
            )
            VALUES (
                (
                    SELECT id
                    FROM habits
                    WHERE name = $habitName
                ),
                $time,
                $date,
                $message
            );
        """;

        var parameters = new Dictionary<string, string>{
            {"$habitName", habitName},
            {"$time", time},
            {"$date", date},
            {"$message", message}
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

    public static Commit Get(int commitId)
    {
        using var connection = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT commits.id,
                habits.name,
                time,
                date,
                message
            FROM commits
            INNER JOIN habits
            ON habits.id = commits.habit_id
            WHERE commits.id = $commit_id
        """;
        command.Parameters.AddWithValue("$commit_id", commitId.ToString());

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var commit = new Commit(
                reader.GetInt32(0),
                reader.GetString(1),
                TimeSpan.Parse(reader.GetString(2)),
                DateTime.Parse(reader.GetString(3)),
                reader.GetString(4)
            );
            return commit;
        }
        throw new Exceptions.CommitNotFoundException(commitId);
    }

    public static List<Commit> GetAll()
    {
        using var connection = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT commits.id,
                habits.name,
                time,
                date,
                message
            FROM commits
            INNER JOIN habits
            ON habits.id = commits.habit_id
        """;

        var commits = new List<Commit>();
        Commit commit;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            commit = new Commit(
                reader.GetInt32(0),
                reader.GetString(1),
                TimeSpan.Parse(reader.GetString(2)),
                DateTime.Parse(reader.GetString(3)),
                reader.GetString(4)
            );
            commits.Add(commit);
        }

        return commits;
    }

    public static List<Commit> GetByHabit(string habitName)
    {
        using var connection = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT *
            FROM commits
            WHERE habit_id = (
                SELECT id
                FROM habits
                WHERE name = $habit_name
            );
        """;
        command.Parameters.AddWithValue("habit_name", habitName);

        var commits = new List<Commit>();
        Commit commit;

        try
        {
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                commit = new Commit(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    TimeSpan.Parse(reader.GetString(2)),
                    DateTime.Parse(reader.GetString(3)),
                    reader.GetString(4)
                );
                commits.Add(commit);
            }
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            throw new Exceptions.HabitNotFoundException(habitName);
        }

        return commits;
    }

    public static void UpdateHabit(
        int commitId,
        string habitName
    )
    {
        string command = """
            UPDATE commits
            SET habit_id = (
                SELECT id
                FROM habits
                WHERE name = $habit_name
            )
            WHERE id = $commit_id;
        """;

        var parameters = new Dictionary<string, string>{
            {"$habit_name", habitName},
            {"$commit_id", commitId.ToString()}
        };

        try
        {
            Database.ExecuteCommand(command, parameters);
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            throw new Exceptions.CommitNotFoundException(commitId);
        }
    }

    public static void UpdateTime(
        int commitId,
        string time
    )
    {
        string command = """
            UPDATE commits
            SET time = $time
            WHERE id = $commit_id
        """;

        var parameters = new Dictionary<string, string>{
            {"$time", time},
            {"$commit_id", commitId.ToString()}
        };

        try
        {
            Database.ExecuteCommand(command, parameters);
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            throw new Exceptions.CommitNotFoundException(commitId);
        }
    }

    public static void UpdateDate(
        int commitId,
        string date
    )
    {
        string command = """
            UPDATE commits
            SET date = $date
            WHERE id = $commit_id
        """;

        var parameters = new Dictionary<string, string>{
            {"$date", date},
            {"$commit_id", commitId.ToString()}
        };

        try
        {
            Database.ExecuteCommand(command, parameters);
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            throw new Exceptions.CommitNotFoundException(commitId);
        }
    }

    public static void UpdateMessage(
        int commitId,
        string message 
    )
    {
        string command = """
            UPDATE commits
            SET message = $message
            WHERE id = $commit_id
        """;

        var parameters = new Dictionary<string, string>{
            {"$message", message},
            {"$commit_id", commitId.ToString()}
        };

        try
        {
            Database.ExecuteCommand(command, parameters);
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            throw new Exceptions.CommitNotFoundException(commitId);
        }
    }

    public static void Remove(int commitId)
    {
        string command = """
            DELETE FROM commits
            WHERE id = $commit_id
        """;
        var parameters = new Dictionary<string, string>{
            {"$commit_id", commitId.ToString()}
        };

        try
        {
            Database.ExecuteCommand(command, parameters);
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            throw new Exceptions.CommitNotFoundException(commitId);
        }
    }
}
