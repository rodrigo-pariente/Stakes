class CommitTable
{
    public record Commit(
        int Id,
        int HabitId,
        TimeSpan EffortTime,
        DateTime CommitTime,
        string Message
    );

    public static void Add(
        int habitId,
        string effortTime,
        string commitTime,
        string message = ""
    )
    {

        string command = """
            INSERT INTO commits (
                habit_id,
                effort_time,
                commit_time,
                message
            )
            VALUES (
                $habitId,
                $effortTime,
                $commitTime,
                $message
            );
        """;

        var parameters = new Dictionary<string, string>{
            {"$habitId", habitId.ToString()},
            {"$effortTime", effortTime},
            {"$commitTime", commitTime},
            {"$message", message}
        };

        Database.ExecuteCommand(command, parameters);
    }

    public static Commit Get(int commitId)
    {
        using var connection = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT *
            FROM commits
            WHERE id = $commit_id;
        """;

        command.Parameters.AddWithValue("$commit_id", commitId.ToString());

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var commit = new Commit(
                reader.GetInt32(0),
                reader.GetInt32(1),
                TimeSpan.Parse(reader.GetString(2)),
                DateTime.Parse(reader.GetString(3)),
                reader.GetString(4)
            );
            return commit;
        }
        // will it ever happen?
        throw new IndexOutOfRangeException("No quote of given id!");
    }

    public static List<Commit> GetAll()
    {
        using var connection = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT *
            FROM commits
        """;

        var commits = new List<Commit>();
        Commit commit;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            commit = new Commit(
                reader.GetInt32(0),
                reader.GetInt32(1),
                TimeSpan.Parse(reader.GetString(2)),
                DateTime.Parse(reader.GetString(3)),
                reader.GetString(4)
            );
            commits.Add(commit);
        }

        return commits;
    }

    public static List<Commit> GetOfHabit(int habitId)
    {
        using var connection = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT *
            FROM commits
            WHERE habit_id = $habit_id;
        """;
        // I trust sqlite to be faster, than C#, and by so
        // I'll not implement GetOfHabit = filter(GetAll)
        command.Parameters.AddWithValue("habit_id", habitId.ToString());

        var commits = new List<Commit>();
        Commit commit;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            commit = new Commit(
                reader.GetInt32(0),
                reader.GetInt32(1),
                TimeSpan.Parse(reader.GetString(2)),
                DateTime.Parse(reader.GetString(3)), // Culprit!!!!!
                reader.GetString(4)
            );
            commits.Add(commit);
        }

        return commits;
    }

    public static void Update(
        int commitId,
        int habitId = -1,
        string effortTime = "",
        string commitTime = "",
        string message = ""
    )
    {
        using var connection = Database.GetConnection();

        // Commit commit = Get(commitId);

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT *
            FROM commits
            WHERE id = $commit_id
        """;
        command.Parameters.AddWithValue(
            "$commit_id", commitId.ToString()
        );
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            if (habitId == -1)
            {
                habitId = reader.GetInt32(1);
            }
            if (effortTime.Equals(string.Empty))
            {
                effortTime = reader.GetString(2);
            }
            if (commitTime.Equals(string.Empty))
            {
                commitTime = reader.GetString(3);
            }
            if (message.Equals(string.Empty))
            {
                message = reader.GetString(4);
            }
        }

        string cmd = """
            UPDATE commits
            SET habit_id = $habit_id,
                effort_time = $effort_time,
                commit_time = $commit_time,
                message = $message
            WHERE id = $commit_id;
        """;

        var parameters = new Dictionary<string, string>{
            {"$habit_id", habitId.ToString()},
            {"$effort_time", effortTime},
            {"$commit_time", commitTime},
            {"$message", message},
            {"$commit_id", commitId.ToString()}
        };

        Database.ExecuteCommand(cmd, parameters);
    }

    public static void Remove(int commitId)
    {
        string command = """
            DELETE FROM commits
            WHERE id = $commit_id;
        """;
        var parameters = new Dictionary<string, string>{
            {"$commit_id", commitId.ToString()}
        };

        Database.ExecuteCommand(command, parameters);
    }
}

